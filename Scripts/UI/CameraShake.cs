using Godot;
using System;
using System.Threading.Tasks;

public partial class CameraShake : Camera3D
{
	public async Task StartShake(float durationSeconds, float magnitude)
	{
		Vector3 originalPosition = Position;
		Random random = new Random();
		float elapsed = 0.0f;

		while (elapsed < durationSeconds)
		{
			float offsetX = (float)(random.NextDouble() * 2 - 1) * magnitude;
			float offsetY = (float)(random.NextDouble() * 2 - 1) * magnitude;
			Position = originalPosition + new Vector3(offsetX, offsetY, 0);

			await Task.Delay(106); // Approximate 60 FPS
			elapsed += 0.16f;
		}

		Position = originalPosition;
	}
}
