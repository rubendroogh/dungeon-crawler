using Godot;
using System.Threading.Tasks;

public partial class IntroCutscene : Control
{
    [Export(PropertyHint.MultilineText)] public string IntroTextStory { get; set; }
    [Export(PropertyHint.MultilineText)] public string IntroTextSingular { get; set; }
    [Export(PropertyHint.MultilineText)] public string IntroTextPlural { get; set; }
    private bool Continuing;

    public Task Start()
    {
        var story = FindChild("StoryText") as AnimatedText;
        var alignment = FindChild("AlignmentText") as AnimatedText;
        story.Text = IntroTextStory;
        story.Start();
        var domains = PlayerManager.Instance.GetPlayer().CharacterData.Alignment;
        alignment.Text = domains.Length == 1
            ? string.Format(IntroTextSingular, domains[0].ToRichString())
            : string.Format(IntroTextPlural, domains[0].ToRichString(), domains[1].ToRichString());
        alignment.Start();
        return Task.CompletedTask;
    }

    public override void _Input(InputEvent input)
    {
        if (!IsVisibleInTree() || Continuing) return;
        if (input is InputEventKey key && key.Pressed && !key.Echo && (key.Keycode == Key.Space || key.Keycode == Key.Enter)
            || input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            Continuing = true;
            GetViewport().SetInputAsHandled();
            _ = TransitionManager.Instance.CutsceneToGame();
        }
    }
}
