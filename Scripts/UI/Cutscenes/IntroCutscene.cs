using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class IntroCutscene : Control
{
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
    /// Starts the intro cutscene sequence, and resolves when it is done.
    /// </summary>
    public async Task Start()
    {
        if (FindChild("IntroText") is not FullScreenText textNode)
        {
            GD.PrintErr("Intro text not found!");
            return;
        }

        var alignment = Managers.PlayerManager.GetPlayer().CharacterData.Alignment;
        var singleAlignment = alignment.Length == 1;

        var introTextGodsTemplate = singleAlignment ? IntroTextSingular : IntroTextPlural;
        var fullIntroText = singleAlignment
            ? string.Format(introTextGodsTemplate, alignment[0])
            : string.Format(introTextGodsTemplate, alignment[0], alignment[1]);

        GD.Print(textNode.Text);
        await this.Delay(1000);
        textNode.Text = fullIntroText;
        textNode.Start();

        // Controls when to go next to the game.
    }
}
