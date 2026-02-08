using Godot;
using System;
using System.Threading.Tasks;

public partial class SoundEffectManager : Node
{
    public static SoundEffectManager Instance { get; private set; }

    /// <summary>
    /// The sound effect for button clicks.
    /// </summary>
    [Export]
    private AudioStream ButtonClickSound { get; set; }

    /// <summary>
    /// The AudioStreamPlayer node used to play sound effects.
    /// </summary>
    private AudioStreamPlayer AudioStreamPlayer { get; set; }

    public override void _Ready()
    {
        Instance = this;
        AudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        if (AudioStreamPlayer == null)
        {
            GD.PrintErr("AudioStreamPlayer node not found in SoundEffectManager.");
        }
    }

    /// <summary>
    /// Plays the button click sound effect with modified pitch and volume.
    /// </summary>
    public async Task PlayButtonClick()
    {
        // TODO: Edit click sound directly to have lower pitch and volume
        AudioStreamPlayer.PitchScale = 0.65f;
        AudioStreamPlayer.VolumeDb = -25f;
        await PlaySoundEffect(ButtonClickSound);

        // Reset pitch and volume to default values after playing the sound
        AudioStreamPlayer.PitchScale = 1.0f;
        AudioStreamPlayer.VolumeDb = 0.0f;
    }

    /// <summary>
    /// Plays the given sound effect and waits for it to finish.
    /// </summary>
    public async Task PlaySoundEffect(AudioStream soundEffect)
    {
        if (AudioStreamPlayer == null)
        {
            GD.PrintErr("AudioStreamPlayer is not initialized.");
            return;
        }

        if (soundEffect == null)
        {
            GD.PrintErr("Invalid sound effect provided.");
            return;
        }

        AudioStreamPlayer.Stream = soundEffect;
        AudioStreamPlayer.Play();

        // Wait for the sound effect to finish playing
        await ToSignal(AudioStreamPlayer, "finished");
    }
}
