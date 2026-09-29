using Jotunn.Managers;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace BrudvikStackedChest.Utils
{
    /// <summary>
    /// Translates the mod's texts with the game's language system. The translations are embedded in the plugin and
    /// handed to Jotunn, which adds them to the game's localization.
    /// </summary>
    public static class Texts
    {
        private static readonly string[] Languages = { "English", "Norwegian" };

        /// <summary>
        /// Registers the embedded translation files. Call once from the plugin's Awake.
        /// </summary>
        /// <param name="pluginName">The plugin name, which is the root namespace of the embedded resources.</param>
        public static void Register(string pluginName)
        {
            var localization = LocalizationManager.Instance.GetLocalization();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var language in Languages)
            {
                var resourceName = $"{pluginName}.Translations.{language}.json";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Jotunn.Logger.LogError($"Missing embedded translation {resourceName}");
                    continue;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                localization.AddJsonFile(language, reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Gets the translation of one of the mod's texts in the player's language.
        /// </summary>
        /// <param name="key">The translation key, without the leading <c>$</c>.</param>
        /// <param name="args">Values for the <c>{0}</c>, <c>{1}</c> placeholders in the text.</param>
        /// <returns>The translated text.</returns>
        public static string Get(string key, params object[] args)
        {
            var template = Localize("$" + key);
            return args.Length == 0 ? template : string.Format(CultureInfo.CurrentCulture, template, args);
        }

        /// <summary>
        /// Translates every <c>$token</c> in a text, such as a chest name or a game item name.
        /// </summary>
        /// <param name="text">The text to translate.</param>
        /// <returns>The translated text, or the text itself when the game's localization is not loaded.</returns>
        public static string Localize(string text)
        {
            return Localization.instance == null ? text : Localization.instance.Localize(text);
        }
    }
}
