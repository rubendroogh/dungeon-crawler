using Godot;
using System;

/// <summary>
/// Handles the UI for the full screen text cutscene scenes.
/// Displays the text character-by-character while preserving rich text formatting.
/// </summary>
public partial class AnimatedText : RichTextLabel
{
    /// <summary>
    /// Defines how many characters should be shown per second if not fast-forwarding.
    /// </summary>
    [Export]
    private float CharactersPerSecond = 40f;

    /// <summary>
    /// The amount of characters shown so far.
    /// </summary>
    private float CharProgress;

    /// <summary>
    /// Whether the text is currently animating.
    /// </summary>
    private bool IsAnimating;

    public override void _Ready()
    {
        Text = string.Empty;
    }

    public void Start()
    {
        // Make sure rich text is enabled
        BbcodeEnabled = true;

        // Cache full text length before hiding it
        VisibleCharacters = 0;

        CharProgress = 0f;
        IsAnimating = true;
    }

    public override void _Process(double delta)
    {
        if (!IsAnimating)
        {
            return;
        }

        CharProgress += CharactersPerSecond * (float)delta;
        VisibleCharacters = Mathf.FloorToInt(CharProgress);

        if (VisibleCharacters >= GetTotalCharacterCount())
        {
            VisibleCharacters = -1; // show all characters
            IsAnimating = false;
        }
    }

    /// <summary>
    /// Instantly finishes the text animation.
    /// Useful for skip / fast-forward input.
    /// </summary>
    public void RevealAll()
    {
        VisibleCharacters = -1;
        IsAnimating = false;
    }

    /// <summary>
    /// Restarts the animation from the beginning.
    /// </summary>
    public void Restart()
    {
        CharProgress = 0f;
        VisibleCharacters = 0;
        IsAnimating = true;
    }
}
