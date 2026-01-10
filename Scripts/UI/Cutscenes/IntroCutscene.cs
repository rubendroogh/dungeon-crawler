using Godot;
using System.Threading.Tasks;

public partial class IntroCutscene : Control
{
    /// <summary>
    /// The story text to show before the godly alignment.
    /// </summary>
    [Export(PropertyHint.MultilineText)]
    public string IntroTextStory { get; set; }

    /// <summary>
    /// The introduction text to show if the player is only aligned to one god.
    /// </summary>
    [Export(PropertyHint.MultilineText)]
    public string IntroTextSingular { get; set; }

    /// <summary>
    /// The introduction text to show if the player is aligned to two gods.
    /// </summary>
    [Export(PropertyHint.MultilineText)]
    public string IntroTextPlural { get; set; }

    /// <summary>
    /// True if the cutscene can be skipped by pressing space.
    /// </summary>
    private bool Skippable { get; set; } = false;

    /// <summary>
    /// Starts the intro cutscene sequence, and resolves when it is done.
    /// </summary>
    public async Task Start()
    {
        if (FindChild("AlignmentText") is not AnimatedText alignmentText)
        {
            GD.PrintErr("AlignmentText not found!");
            return;
        }

        if (FindChild("StoryText") is not AnimatedText storyText)
        {
            GD.PrintErr("StoryText not found!");
            return;
        }

        if (FindChild("ContinueText") is not RichTextLabel continueText)
        {
            GD.PrintErr("ContinueText not found!");
            return;
        }

        continueText.AddThemeColorOverride("default_color", Colors.Transparent);

        await this.Delay(1_000);
        storyText.Text = IntroTextStory;
        storyText.Start();

        await this.Delay(9_000);
        // TODO: Add a pling sound when the alignment is announced
        alignmentText.Text = GetAlignmentText();
        alignmentText.Start();

        // Controls when to go next to the game.
        await this.Delay(5_000);
        continueText.AddThemeColorOverride("default_color", Colors.Gray);
        Skippable = true;
    }

    /// <summary>
    /// Handle button presses to skip cutscene.
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (Skippable && eventKey.Pressed && eventKey.Keycode == Key.Space)
            {
                _ = Managers.TransitionManager.CutsceneToGame();
            }
        }
    }

    /// <summary>
    /// Gets the text to show the player's alignment.
    /// </summary>
    private string GetAlignmentText()
    {
        var alignment = Managers.PlayerManager.GetPlayer().CharacterData.Alignment;
        var singleAlignment = alignment.Length == 1;

        var introTextGodsTemplate = singleAlignment ? IntroTextSingular : IntroTextPlural;
        return singleAlignment
            ? string.Format(introTextGodsTemplate, alignment[0].ToRichString())
            : string.Format(introTextGodsTemplate, alignment[0].ToRichString(), alignment[1].ToRichString());
    }
}
