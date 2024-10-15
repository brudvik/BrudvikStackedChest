using UnityEngine;

namespace BrudvikStackedChest.Utils
{
    public static class SharedUtils
    {
        // Convert RGB values to a Color object
        public static Color ColorFromRGB(byte r, byte g, byte b, float a = 1.0f)
        {
            byte alpha = (byte)(a * 255);
            return new Color32(r, g, b, alpha);
        }
    }
}
