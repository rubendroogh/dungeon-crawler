using Godot;
using DungeonRPG.Blessings.Enums;

public static class DomainExtensions
{
    /// <summary>
    /// Gets the bbcode formatted colored domain name.
    /// </summary>
    public static string ToRichString(this Domain domain)
    {
        return string.Format(
			"[color={0}]{1}[/color]",
			domain.GetColor().ToHexString(),
			domain);
    }

    /// <summary>
    /// Gets the color associated to the domain.
    /// </summary>
    public static Color GetColor(this Domain domain)
    {
        var baseCalinaColor = new Color(1f, 0.5f, 0.5f); // Light Red
		var baseHaminColor = new Color(0.5f, 0.5f, 1f); // Light Blue
		var baseJaddisColor = new Color(0.5f, 1f, 0.5f); // Light Green
		var baseZerColor = new Color(0.75f, 0.5f, 1f); // Light Purple

		return domain switch
		{
			Domain.Calina => baseCalinaColor,
			Domain.Hamin => baseHaminColor,
			Domain.Jaddis => baseJaddisColor,
			Domain.Zer => baseZerColor,
			_ => Colors.White,
		};
    }
}
