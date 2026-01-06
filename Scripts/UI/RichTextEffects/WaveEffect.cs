using Godot;

[Tool]
public partial class WaveEffect : RichTextEffect
{
	string bbcode = "wave";

	public override bool _ProcessCustomFX(CharFXTransform charFX)
	{
		float time = (float)charFX.Env["time"];
		float speed = charFX.Env.ContainsKey("speed") ? (float)charFX.Env["speed"] : 4.0f;
		float height = charFX.Env.ContainsKey("height") ? (float)charFX.Env["height"] : 8.0f;

		// Each character gets a phase offset using its index
		float wave = Mathf.Sin(time * speed + charFX.RelativeIndex * 0.5f);

		charFX.Offset = new Vector2(
			charFX.Offset.X,
			charFX.Offset.Y + wave * height
		);

		return true;
	}

}
