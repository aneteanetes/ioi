using System.Globalization;
using Godot;

internal static class ColorExtensions
{
    public static string ToHexString(this Color color)
        => "#"+color.ToHtml();
    
    public static Color AsColor(this string hexcolor)
    {
        hexcolor = hexcolor.Replace("#", "");
        
        byte a=255, r, g, b;

        if (hexcolor.Length > 6)
        {
            a = byte.Parse(hexcolor.Substring(0, 2), NumberStyles.HexNumber);
            r = byte.Parse(hexcolor.Substring(2, 2), NumberStyles.HexNumber);
            g = byte.Parse(hexcolor.Substring(4, 2), NumberStyles.HexNumber);
            b = byte.Parse(hexcolor.Substring(6, 2), NumberStyles.HexNumber);
        }
        else
        {
            r = byte.Parse(hexcolor.Substring(0, 2), NumberStyles.HexNumber);
            g = byte.Parse(hexcolor.Substring(2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hexcolor.Substring(4, 2), NumberStyles.HexNumber);
        }
        
        return Color.Color8(r, g, b, a);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    /// <param name="alpha">normilized</param>
    /// <returns></returns>
    public static Color SetAlpha(this Color color, float alpha)
    {
        return new Color(color, alpha);
    }

    public static Vector4 Normalize(this Color color)
    {
        return new Vector4(1f/color.R, 1f/color.G, 1f/color.B, 1f/color .A);
    }
}
