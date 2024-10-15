using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Utility class for loading assets, specifically textures, from embedded resources.
    /// </summary>
    public static class AssetUtilsExtended
    {
        /// <summary>
        /// Loads a Texture2D object from an embedded resource.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to load.</param>
        /// <returns>A Texture2D object created from the embedded resource.</returns>
        /// <exception cref="Exception">Thrown if the resource is not found or the texture fails to load.</exception>
        public static Texture2D LoadTextureFromEmbeddedResource(string resourceName)
        {
            // Get the currently executing assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Open a stream to the embedded resource
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                // Throw an exception if the resource is not found
                if (stream == null)
                    throw new Exception($"Resource {resourceName} not found.");

                // Copy the resource stream to a memory stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    byte[] data = memoryStream.ToArray();

                    // Create a new Texture2D object
                    Texture2D texture = new Texture2D(2, 2);

                    // Load the image data into the Texture2D object
                    if (texture.LoadImage(data))
                    {
                        return texture; // Return the loaded texture
                    }
                    else
                    {
                        // Throw an exception if the texture fails to load
                        throw new Exception("Failed to load texture from data.");
                    }
                }
            }
        }
    }
}
