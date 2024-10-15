using BrudvikStackedChest.Extensions;
using UnityEngine;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Helper class for loading sprites from embedded resources.
    /// </summary>
    public class SpriteLoader
    {
        /// <summary>
        /// The name of the plugin, used to locate embedded resources.
        /// </summary>
        private readonly string PluginName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteLoader"/> class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin, used to locate embedded resources.</param>
        public SpriteLoader(string pluginName)
        {
            PluginName = pluginName;
        }

        /// <summary>
        /// Loads a sprite from an embedded resource.
        /// </summary>
        /// <param name="filename">The filename of the embedded resource to load.</param>
        /// <returns>A <see cref="Sprite"/> object created from the embedded resource.</returns>
        public Sprite Load(string filename)
        {
            // Construct the full resource name using the plugin name and filename
            string resourceName = $"{PluginName}.Assets.{filename}";

            // Load the texture from the embedded resource and convert it to a sprite
            return AssetUtilsExtended.LoadTextureFromEmbeddedResource(resourceName).ConvertToSprite();
        }
    }
}
