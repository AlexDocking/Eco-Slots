using Eco.Shared.Utils;
using System.Linq;

namespace Parts
{
    public static class ColorUtility
    {
        public static string HexRGB(this Color colour) => RGBHex(colour.HexRGBA);
        public static string RGBHex(string hex)
        {
            if (!IsValidColourHex(hex))
            {
                return "#000000";
            }
            if (!hex.StartsWith('#'))
            {
                hex = "#" + hex;
            }
            hex = hex.PadRight(9);
            hex = hex.Substring(0, 7).ToUpper();
            return hex;
        }
        public static bool IsValidColourHex(string colourHex)
        {
            if (string.IsNullOrEmpty(colourHex)) return false;

            colourHex = colourHex.TrimStart('#');
            if (colourHex.Length != 6 && colourHex.Length != 8) return false;

            colourHex = colourHex.ToLower();
            return colourHex.All(c => (c >= 'a' && c <= 'f') || (c >= '0' && c <= '9'));
        }
        public static Color? FromHex(string hex)
        {
            if (IsValidColourHex(hex))
            {
                return new Color(hex);
            }
            return null;
        }
    }
}
