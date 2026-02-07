using System.Threading.Tasks;
using Godot;

public partial class TransitionManager : Node
{
    /// <summary>
    /// The root node of the world scene.
    /// </summary>
    private Node WorldNode { get; set; }

    /// <summary>
    /// The root node of the main game HUD.
    /// </summary>
    private CanvasItem HUDNode { get; set; }

    /// <summary>
    /// The root node of the character creation UI.
    /// </summary>
    private CanvasItem CharacterCreationNode { get; set; }

    /// <summary>
    /// The root node of the reward selection UI.
    /// </summary>
    private CanvasItem RewardSelectionNode { get; set; }

    /// <summary>
    /// The cutscene node (for intro text only for now).
    /// </summary>
    private IntroCutscene CutsceneNode { get; set; }

    /// <summary>
    /// The duration of fade transitions.
    /// </summary>
    private float FadeDuration = 0.5f;

    public override void _Ready()
    {
        WorldNode = GetTree().Root.GetNode("Root/World");
        HUDNode = GetTree().Root.GetNode("Root/UI/HUD") as CanvasItem;
        CharacterCreationNode = GetTree().Root.GetNode("Root/UI/CharacterCreation") as CanvasItem;
        RewardSelectionNode = GetTree().Root.GetNode("Root/UI/RewardSelection") as CanvasItem;
        CutsceneNode = GetTree().Root.GetNode("Root/UI/IntroCutscene") as IntroCutscene;
    }

    /// <summary>
    /// Transitions to the character creation screen by hiding the world and HUD nodes.
    /// </summary>
    public async Task ToCharacterCreation()
    {
        await SetWorldVisibility(false);
        await SetRewardSelectionVisibility(false);
        await SetCharacterCreationVisibility(true);
    }

    /// <summary>
    /// Transitions from character creation to the intro cutscene.
    /// </summary>
    public async Task CharacterCreationToCutscene()
    {
        await SetCharacterCreationVisibility(false, true);
        await SetRewardSelectionVisibility(false);

        // Go to cutscene and set text based on build
        CutsceneNode.Visible = true;
        await CutsceneNode.Start();
    }

    /// <summary>
    /// Transitions from the intro cutscene to the main game by showing the world and HUD nodes.
    /// </summary>
    public async Task CutsceneToGame()
    {
        await SetCutsceneVisibility(false, true);
        await SetWorldVisibility(true);

        // Temporarily immediately start the battle
        // This is a placeholder for actual game logic to start properly in the dungeon
        Managers.BattleManager.InitializeBattle();
    }

    /// <summary>
    /// Transitions to the reward selection screen and generates 3 rewards.
    /// </summary>
    public async Task ToRewardSelection()
    {
        await SetWorldVisibility(false);
        await SetRewardSelectionVisibility(true);

        Managers.RewardSelectionManager.GenerateRewards(3);
    }

    /// <summary>
    /// Transitions from reward selection to the game by showing the world and HUD nodes.
    /// </summary>
    public async Task RewardSelectionToGame()
    {
        await SetRewardSelectionVisibility(false);
        await SetWorldVisibility(true);

        // Temporarily immediately start the battle
        Managers.BattleManager.InitializeBattle();
    }

    /// <summary>
    /// Sets the visibility of the world and HUD nodes.
    /// </summary>
    /// <param name="value">True if the world and HUD should be visible.</param>
    private async Task SetWorldVisibility(bool value, bool fade = false)
    {
        if (WorldNode == null || HUDNode == null)
        {
            GD.PrintErr("WorldNode or HUDNode is null! (or both!)");
            return;
        }

        // Toggle the visibility of the HUD
        if (fade)
        {
            await FadeCanvasItem(HUDNode, value, FadeDuration);
        }
        else
        {
            HUDNode.Visible = value;
            HUDNode.SetProcess(value);
        }
    }

    /// <summary>
    /// Sets the visibility of the character creation UI.
    /// </summary>
    /// <param name="value">True if the character creation UI should be visible.</param>
    private async Task SetCharacterCreationVisibility(bool value, bool fade = false)
    {
        // Hide or show the character creation UI
        if (CharacterCreationNode == null)
        {
            GD.PrintErr("CharacterCreationNode is null!");
            return;
        }

        if (fade)
        {
            await FadeCanvasItem(CharacterCreationNode, value, FadeDuration);
        }
        else
        {
            CharacterCreationNode.Visible = value;
            CharacterCreationNode.SetProcess(value);
        }
    }

    /// <summary>
    /// Sets the visibility of the reward selection UI.
    /// </summary>
    /// <param name="value">True if the reward selection UI should be visible.</param>
    private async Task SetRewardSelectionVisibility(bool value, bool fade = false)
    {
        // Hide or show the reward selection UI
        if (RewardSelectionNode == null)
        {
            GD.PrintErr("RewardSelectionNode is null!");
            return;
        }

        if (fade)
        {
            await FadeCanvasItem(RewardSelectionNode, value, FadeDuration);
        }
        else
        {
            RewardSelectionNode.Visible = value;
            RewardSelectionNode.SetProcess(value);
        }
    }

    /// <summary>
    /// Sets the visibility of the cutscene UI.
    /// </summary>
    /// <param name="value">True if the cutscene UI should be visible.</param>
    private async Task SetCutsceneVisibility(bool value, bool fade = false)
    {
        // Hide or show the cutscene UI
        if (CutsceneNode == null)
        {
            GD.PrintErr("CutsceneNode is null!");
            return;
        }

        if (fade)
        {
            await FadeCanvasItem(CutsceneNode, value, FadeDuration);
        }
        else
        {
            CutsceneNode.Visible = value;
            CutsceneNode.SetProcess(value);
        }
    }

    /// <summary>
    /// Fades a CanvasItem in or out over a duration.
    /// </summary>
    private static async Task FadeCanvasItem(CanvasItem item, bool fadeIn, float duration)
    {
        if (item == null)
        {
            return;
        }

        var tween = item.CreateTween();
        if (fadeIn)
        {
            item.Visible = true;
            item.Modulate = new Color(1, 1, 1, 0);
            tween.TweenProperty(item, "modulate:a", 1f, duration);
        }
        else
        {
            tween.TweenProperty(item, "modulate:a", 0f, duration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.In)
                .Connect("finished", Callable.From(() =>
                {
                    item.Visible = false;
                }));
        }

        await item.ToSignal(tween, "finished");
    }   
}
