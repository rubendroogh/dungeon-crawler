using Godot;

public static class ColourExtensions
{
    /// <summary>
    /// Modifies the brightness of a color by a given factor.
    /// </summary>
    public static Color ModifyBrightness(this Color color, float factor)
    {
        return new Color(
            Mathf.Clamp(color.R * factor, 0, 1),
            Mathf.Clamp(color.G * factor, 0, 1),
            Mathf.Clamp(color.B * factor, 0, 1),
            color.A
        );
    }

    /// <summary>
    /// Applies a greyscale effect to the color based on the given intensity.
    /// </summary>
    public static Color ApplyGreyscale(this Color color, float intensity)
    {
        float grey = (color.R + color.G + color.B) / 3;
        return new Color(
            Mathf.Lerp(color.R, grey, intensity),
            Mathf.Lerp(color.G, grey, intensity),
            Mathf.Lerp(color.B, grey, intensity),
            color.A
        );
    }

    /// <summary>
    /// Converts the color to a hexadecimal string representation.
    /// </summary>
    public static string ToHexString(this Color color)
    {
        int r = (int)(color.R * 255);
        int g = (int)(color.G * 255);
        int b = (int)(color.B * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
