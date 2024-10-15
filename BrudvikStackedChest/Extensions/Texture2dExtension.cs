using UnityEngine;

namespace BrudvikStackedChest.Extensions
{
    /// <summary>
    /// Extension methods for the Texture2D class.
    /// </summary>
    public static class Texture2dExtension
    {
        /// <summary>
        /// Converts a Texture2D object to a Sprite object.
        /// </summary>
        /// <param name="texture">The Texture2D object to convert.</param>
        /// <returns>A Sprite object created from the Texture2D.</returns>
        public static Sprite ConvertToSprite(this Texture2D texture)
        {
            // Create a new Sprite from the given Texture2D
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero
            );
        }
    }
}
