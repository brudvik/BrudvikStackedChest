using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// World-wide record of unlocked and discovered items, and of how much of each item the chests hold towards
    /// unlocking it. The server owns the record and stores it in files per world; clients keep a synchronized copy.
    /// </summary>
    public class WorldProgress
    {
        private const int RequestAll = 0;
        private const int AddKeys = 1;
        private const int FullRecord = 2;
        private const int StockReport = 3;
        private const int StockBest = 4;
        private const string UnlockPrefix = "u:";
        private const string DiscoveryPrefix = "d:";
        private const int MaxKeyLength = 128;
        private const int MaxKeysPerPackage = 10000;
        private const int MaxChestIdLength = 64;
        private const int MaxStockAmount = 1000000;
        private const int MaxChests = 2000;
        private const int MaxItemsPerChest = 300;

        private readonly HashSet<string> keys = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, int>> stockByChest = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> bestStock = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, int>> reported = new(StringComparer.Ordinal);
        private readonly string saveFolder;
        private readonly CustomRPC rpc;
        private ZNet? session;
        private string? savePath;
        private string? stockPath;
        private bool hasFullRecord;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldProgress"/> class and registers its network RPC.
        /// </summary>
        /// <param name="pluginName">The plugin name, used for the save folder and the RPC name.</param>
        public WorldProgress(string pluginName)
        {
            saveFolder = Path.Combine(Paths.ConfigPath, pluginName);
            rpc = NetworkManager.Instance.AddRPC($"{pluginName}_WorldProgress", OnServerReceive, OnClientReceive);
        }

        /// <summary>
        /// Gets a value indicating whether the record is complete: always on the server, and on a client once the
        /// server has sent it. Until then a client cannot tell unlocked items from locked ones.
        /// </summary>
        public bool IsReady => EnsureSession() && hasFullRecord;

        /// <summary>
        /// Raised with the item prefab name when another player in the world unlocks an item.
        /// </summary>
        public event Action<string>? ItemUnlocked;

        /// <summary>
        /// Checks whether a full stack of the item has been stored in a chest in this world.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>True if the item is unlocked.</returns>
        public bool IsUnlocked(string prefabName) => Contains(UnlockPrefix + prefabName);

        /// <summary>
        /// Checks whether any player in this world has discovered the item.
        /// </summary>
        /// <param name="itemToken">The item's localization token (<c>m_shared.m_name</c>).</param>
        /// <returns>True if the item is discovered.</returns>
        public bool IsDiscovered(string itemToken) => Contains(DiscoveryPrefix + itemToken);

        /// <summary>
        /// Unlocks an item for the whole world.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        public void Unlock(string prefabName) => Add(new[] { UnlockPrefix + prefabName });

        /// <summary>
        /// Records items as discovered for the whole world.
        /// </summary>
        /// <param name="itemTokens">The items' localization tokens (<c>m_shared.m_name</c>).</param>
        public void Discover(IEnumerable<string> itemTokens) => Add(itemTokens.Select(token => DiscoveryPrefix + token));

        /// <summary>
        /// Gets the largest amount of an item that any single chest in the world holds towards unlocking it.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>The amount in the chest that is closest to unlocking the item, or 0 if no chest holds any.</returns>
        public int GetBestStored(string prefabName)
        {
            return EnsureSession() && bestStock.TryGetValue(prefabName, out var amount) ? amount : 0;
        }

        /// <summary>
        /// Tells the server how much a chest holds of the items it can still unlock. Call this from the owner of the
        /// chest; nothing is sent when the amounts have not changed since the last report.
        /// </summary>
        /// <param name="chestId">The chest's network id.</param>
        /// <param name="amounts">The stored amount per item prefab name; empty when the chest is removed.</param>
        public void ReportStock(string chestId, Dictionary<string, int> amounts)
        {
            if (!EnsureSession()) return;
            if (reported.TryGetValue(chestId, out var last) && SameAmounts(last, amounts)) return;

            reported[chestId] = new Dictionary<string, int>(amounts, StringComparer.Ordinal);
            if (ZNet.instance.IsServer())
            {
                ApplyStock(chestId, amounts);
                return;
            }

            var package = new ZPackage();
            package.Write(StockReport);
            package.Write(chestId);
            WriteAmounts(package, amounts);
            rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
        }

        /// <summary>
        /// Asks the server for the full record. Call this when the local player has joined a world.
        /// </summary>
        public void RequestSync()
        {
            if (!EnsureSession() || ZNet.instance.IsServer()) return;

            var package = new ZPackage();
            package.Write(RequestAll);
            rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), package);
        }

        private bool Contains(string key)
        {
            return EnsureSession() && keys.Contains(key);
        }

        private void Add(IEnumerable<string> candidates)
        {
            AddAndReturnNew(candidates);
        }

        private List<string> AddAndReturnNew(IEnumerable<string> candidates)
        {
            if (!EnsureSession()) return new List<string>();

            var added = candidates.Where(key => keys.Add(key)).ToList();
            if (added.Count == 0) return added;

            if (ZNet.instance.IsServer())
            {
                Persist(added);
                Broadcast(Pack(AddKeys, added));
            }
            else
            {
                rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), Pack(AddKeys, added));
            }
            return added;
        }

        private void RaiseUnlocked(IEnumerable<string> newKeys)
        {
            foreach (var key in newKeys)
            {
                if (key.StartsWith(UnlockPrefix, StringComparison.Ordinal)) ItemUnlocked?.Invoke(key.Substring(UnlockPrefix.Length));
            }
        }

        /// <summary>
        /// Replaces the amounts of one chest on the server and sends the items whose best amount changed to everyone.
        /// </summary>
        private void ApplyStock(string chestId, Dictionary<string, int> amounts)
        {
            stockByChest.TryGetValue(chestId, out var previous);
            if (amounts.Count == 0)
            {
                if (previous == null) return;
                stockByChest.Remove(chestId);
            }
            else
            {
                if (previous == null && stockByChest.Count >= MaxChests) return;
                stockByChest[chestId] = new Dictionary<string, int>(amounts, StringComparer.Ordinal);
            }

            var affected = new HashSet<string>(amounts.Keys, StringComparer.Ordinal);
            if (previous != null) affected.UnionWith(previous.Keys);

            var changes = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var name in affected)
            {
                var best = 0;
                foreach (var chest in stockByChest.Values)
                {
                    if (chest.TryGetValue(name, out var amount) && amount > best) best = amount;
                }

                bestStock.TryGetValue(name, out var current);
                if (best == current) continue;

                if (best == 0) bestStock.Remove(name);
                else bestStock[name] = best;
                changes[name] = best;
            }

            SaveStock();
            if (changes.Count > 0) Broadcast(PackBest(changes));
        }

        /// <summary>
        /// Starts with an empty record for every new network session and loads the saved record on the server.
        /// </summary>
        private bool EnsureSession()
        {
            var current = ZNet.instance;
            if (current == null || ZRoutedRpc.instance == null) return false;
            if (session == current) return true;

            session = current;
            keys.Clear();
            stockByChest.Clear();
            bestStock.Clear();
            reported.Clear();
            savePath = null;
            stockPath = null;
            hasFullRecord = current.IsServer();

            if (current.IsServer() && ZNet.World != null)
            {
                var worldName = string.Concat(ZNet.World.m_name.Split(Path.GetInvalidFileNameChars()));
                savePath = Path.Combine(saveFolder, $"{worldName}_{ZNet.World.m_uid}.txt");
                stockPath = Path.Combine(saveFolder, $"{worldName}_{ZNet.World.m_uid}_stock.txt");
                Load();
                LoadStock();
            }

            return true;
        }

        private void Load()
        {
            try
            {
                if (savePath == null || !File.Exists(savePath)) return;

                foreach (var line in File.ReadAllLines(savePath))
                {
                    if (line.Length > 0) keys.Add(line);
                }

                Jotunn.Logger.LogInfo($"Loaded {keys.Count} unlocked and discovered items for this world.");
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not read world progress from {savePath}: {ex.Message}");
            }
        }

        private void LoadStock()
        {
            try
            {
                if (stockPath == null || !File.Exists(stockPath)) return;

                foreach (var line in File.ReadAllLines(stockPath))
                {
                    var parts = line.Split('\t');
                    if (parts.Length != 3 || !IsValidChestId(parts[0]) || !IsValidName(parts[1]) ||
                        !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount) ||
                        amount <= 0) continue;

                    if (!stockByChest.TryGetValue(parts[0], out var chest))
                    {
                        chest = new Dictionary<string, int>(StringComparer.Ordinal);
                        stockByChest[parts[0]] = chest;
                    }
                    chest[parts[1]] = Math.Min(amount, MaxStockAmount);

                    bestStock.TryGetValue(parts[1], out var best);
                    if (amount > best) bestStock[parts[1]] = chest[parts[1]];
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not read chest stock from {stockPath}: {ex.Message}");
            }
        }

        private void Persist(List<string> added)
        {
            try
            {
                if (savePath == null) return;

                Directory.CreateDirectory(saveFolder);
                File.AppendAllLines(savePath, added);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not save world progress to {savePath}: {ex.Message}");
            }
        }

        private void SaveStock()
        {
            try
            {
                if (stockPath == null) return;

                var lines = new List<string>();
                foreach (var chest in stockByChest)
                {
                    foreach (var item in chest.Value)
                    {
                        lines.Add($"{chest.Key}\t{item.Key}\t{item.Value.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                Directory.CreateDirectory(saveFolder);
                File.WriteAllLines(stockPath, lines);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not save chest stock to {stockPath}: {ex.Message}");
            }
        }

        private void Broadcast(ZPackage package)
        {
            if (ZNet.instance.m_peers.Count > 0) rpc.SendPackage(ZNet.instance.m_peers, package);
        }

        private static ZPackage Pack(int type, ICollection<string> values)
        {
            var package = new ZPackage();
            package.Write(type);
            package.Write(values.Count);
            foreach (var value in values)
            {
                package.Write(value);
            }
            return package;
        }

        private static ZPackage PackBest(Dictionary<string, int> amounts)
        {
            var package = new ZPackage();
            package.Write(StockBest);
            WriteAmounts(package, amounts);
            return package;
        }

        private static void WriteAmounts(ZPackage package, Dictionary<string, int> amounts)
        {
            package.Write(amounts.Count);
            foreach (var pair in amounts)
            {
                package.Write(pair.Key);
                package.Write(pair.Value);
            }
        }

        private static List<string> Unpack(ZPackage package)
        {
            var count = package.ReadInt();
            if (count < 0 || count > MaxKeysPerPackage) return new List<string>();

            var values = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                values.Add(package.ReadString());
            }
            return values;
        }

        private static Dictionary<string, int>? UnpackAmounts(ZPackage package)
        {
            var count = package.ReadInt();
            if (count < 0 || count > MaxKeysPerPackage) return null;

            var amounts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < count; i++)
            {
                var name = package.ReadString();
                amounts[name] = package.ReadInt();
            }
            return amounts;
        }

        private IEnumerator OnServerReceive(long sender, ZPackage package)
        {
            if (EnsureSession())
            {
                var type = package.ReadInt();
                if (type == RequestAll)
                {
                    rpc.SendPackage(sender, Pack(FullRecord, keys.ToList()));
                    rpc.SendPackage(sender, PackBest(bestStock));
                }
                else if (type == AddKeys)
                {
                    RaiseUnlocked(AddAndReturnNew(Unpack(package).Where(IsValidKey)));
                }
                else if (type == StockReport)
                {
                    var chestId = package.ReadString();
                    var amounts = UnpackAmounts(package);
                    if (amounts != null && amounts.Count <= MaxItemsPerChest && IsValidChestId(chestId) &&
                        amounts.All(pair => IsKnownItem(pair.Key) && pair.Value > 0 && pair.Value <= MaxStockAmount))
                    {
                        ApplyStock(chestId, amounts);
                    }
                }
            }
            yield break;
        }

        // Keys from clients end up in the save file, one per line.
        private static bool IsValidKey(string key)
        {
            return key.Length <= MaxKeyLength &&
                   (key.StartsWith(UnlockPrefix, StringComparison.Ordinal) || key.StartsWith(DiscoveryPrefix, StringComparison.Ordinal)) &&
                   key.IndexOfAny(new[] { '\r', '\n' }) < 0;
        }

        // Names from clients end up in the stock file, separated by tabs.
        private static bool IsValidName(string name)
        {
            return name.Length > 0 && name.Length <= MaxKeyLength && name.IndexOfAny(new[] { '\t', '\r', '\n' }) < 0;
        }

        private static bool IsValidChestId(string chestId)
        {
            return chestId.Length > 0 && chestId.Length <= MaxChestIdLength &&
                   chestId.All(c => char.IsDigit(c) || c == ':' || c == '-');
        }

        private static bool IsKnownItem(string name)
        {
            return IsValidName(name) && (ObjectDB.instance == null || ObjectDB.instance.GetItemPrefab(name) != null);
        }

        private static bool SameAmounts(Dictionary<string, int> left, Dictionary<string, int> right)
        {
            if (left.Count != right.Count) return false;

            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out var amount) || amount != pair.Value) return false;
            }
            return true;
        }

        private IEnumerator OnClientReceive(long sender, ZPackage package)
        {
            if (EnsureSession())
            {
                var type = package.ReadInt();
                if (type == FullRecord)
                {
                    keys.UnionWith(Unpack(package));
                    hasFullRecord = true;
                }
                else if (type == AddKeys)
                {
                    RaiseUnlocked(Unpack(package).Where(key => keys.Add(key)).ToList());
                }
                else if (type == StockBest)
                {
                    foreach (var pair in UnpackAmounts(package) ?? new Dictionary<string, int>())
                    {
                        if (pair.Value <= 0) bestStock.Remove(pair.Key);
                        else bestStock[pair.Key] = pair.Value;
                    }
                }
            }
            yield break;
        }
    }
}
