using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// World-wide record of unlocked and discovered items. The server owns the record and stores it in a file per
    /// world; clients keep a synchronized copy.
    /// </summary>
    public class WorldProgress
    {
        private const int RequestAll = 0;
        private const int AddKeys = 1;
        private const int FullRecord = 2;
        private const string UnlockPrefix = "u:";
        private const string DiscoveryPrefix = "d:";
        private const int MaxKeyLength = 128;
        private const int MaxKeysPerPackage = 10000;

        private readonly HashSet<string> keys = new(StringComparer.Ordinal);
        private readonly string saveFolder;
        private readonly CustomRPC rpc;
        private ZNet? session;
        private string? savePath;
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
                Broadcast(added);
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
        /// Starts with an empty record for every new network session and loads the saved record on the server.
        /// </summary>
        private bool EnsureSession()
        {
            var current = ZNet.instance;
            if (current == null || ZRoutedRpc.instance == null) return false;
            if (session == current) return true;

            session = current;
            keys.Clear();
            savePath = null;
            hasFullRecord = current.IsServer();

            if (current.IsServer() && ZNet.World != null)
            {
                var worldName = string.Concat(ZNet.World.m_name.Split(Path.GetInvalidFileNameChars()));
                savePath = Path.Combine(saveFolder, $"{worldName}_{ZNet.World.m_uid}.txt");
                Load();
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

        private void Broadcast(List<string> added)
        {
            if (ZNet.instance.m_peers.Count > 0) rpc.SendPackage(ZNet.instance.m_peers, Pack(AddKeys, added));
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

        private IEnumerator OnServerReceive(long sender, ZPackage package)
        {
            if (EnsureSession())
            {
                if (package.ReadInt() == RequestAll)
                {
                    rpc.SendPackage(sender, Pack(FullRecord, keys.ToList()));
                }
                else
                {
                    RaiseUnlocked(AddAndReturnNew(Unpack(package).Where(IsValidKey)));
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
            }
            yield break;
        }
    }
}
