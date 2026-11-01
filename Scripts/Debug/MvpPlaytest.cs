using System;
using System.Linq;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using Godot;
using Priest.Managers;

/// <summary>
/// Opt-in integration playthrough and screenshot capture, independent of production UI setup.
/// </summary>
public partial class MvpPlaytest : Node
{
    private static int TestStage;
    [Export]
    private ComponentExposer ExplorationExposer;
    [Export]
    private CharacterCreationManager Creation;
    [Export]
    private IntroCutscene Intro;
    [Export]
    private TabContainer ResourceTabs;
    [Export]
    private AddSpellToQueueButton AddSpell;
    [Export]
    private ComponentExposer SpellBookExposer;
    [Export]
    private Button ClearQueue;

    [Export]
    private AudioStreamPlayer2D Music;

    private VBoxContainer Choices { get; set; }
    private Node SpellList { get; set; }
    private LineEdit PlayerNameInput { get; set; }
    private Button SubmitButton { get; set; }

    public override void _Ready()
    {
        if (OS.GetCmdlineUserArgs().Contains("--mvp-test"))
        {
            Choices = ExplorationExposer.GetComponent<VBoxContainer>(Components.SceneChoices);
            SpellList = SpellBookExposer.GetComponent<Node>(Components.SpellListContainer);
            PlayerNameInput = Creation.FindChild(Components.PlayerNameInput) as LineEdit;
            SubmitButton = Creation.FindChild(Components.SubmitButton) as Button;
            CallDeferred(nameof(RunChecks));
        }
    }

    private void Require(bool value, string description)
    {
        if (!value)
        {
            throw new InvalidOperationException(description);
        }

        GD.Print($"PASS: {description}");
    }

    private async Task WaitUntil(Func<bool> condition, string description)
    {
        for (var i = 0; i < 5000; i++)
        {
            if (condition())
            {
                return;
            }

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        throw new TimeoutException(description);
    }

    private async Task ClickChoice(string prefix)
    {
        await WaitUntil(() => !SceneManager.Instance.IsResolvingChoice, "Previous choice completed");
        var button = Choices.GetChildren().OfType<Button>().First(b => b.Text.StartsWith(prefix));
        Require(!button.Disabled, $"Choice available: {prefix}");
        button.EmitSignal(Button.SignalName.Pressed);
        await WaitUntil(() => !SceneManager.Instance.IsResolvingChoice, "Choice completed");
    }

    private async void RunChecks()
    {
        try
        {
            Engine.TimeScale = 40;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (TestStage > 0)
            {
                await CheckReplayAndDefeat();
                return;
            }
            await Capture("01-character-creation");
            foreach (var first in Enum.GetValues<Domain>())
            {
                Require(SpellBookManager.Instance.GetStartingSpellsByDomain([first]).Count >= 2, $"Starting spells for {first}");
                foreach (var second in Enum.GetValues<Domain>().Where(d => d != first))
                {
                    Require(SpellBookManager.Instance.GetStartingSpellsByDomain([first, second]).Count >= 2, $"Starting spells for {first}/{second}");
                }
            }
            PlayerNameInput.Text = "Mart";
            Creation.OnSpinboxValueChanged();
            Require(!SubmitButton.Disabled, "Character form is valid");
            Creation.SubmitForm();
            await WaitUntil(() => Intro.Visible, "Intro opened");
            Intro._Input(new InputEventKey { Pressed = true, Keycode = Key.Space });
            await WaitUntil(() => SceneManager.Instance.CurrentRoom != null, "Dungeon entered");
            Require(SpellBookManager.Instance.AvailableSpells.Count == 2, "Personality grants two starting spells");
            var player = PlayerManager.Instance.GetPlayer();
            Require(player.CharacterData.BaseFocused == 10, "Neutral UI traits map to base value 10");
            await player.ApplyEffect(StatusEffectType.PhysicalInvincibility, 1);
            await player.ClearEffect(StatusEffectType.PhysicalInvincibility);
            Require(player.PhysicalDamageModifiers.All(m => m.Value != 0), "Shield modifier expires");
            await player.ApplyEffect(StatusEffectType.PhysicalInvincibility, 2);
            await player.EndTurn();
            var healthBeforeShield = player.Health;
            await player.Damage(new Damage(4, DamageType.Physical));
            Require(player.Health == healthBeforeShield, "Shield lasts through the enemy turn");
            await player.EndTurn();
            Require(!player.HasEffect(StatusEffectType.PhysicalInvincibility) && player.PhysicalDamageModifiers.All(m => m.Value != 0), "Shield expires on next player turn");
            await player.ApplyEffect(StatusEffectType.Plague, 1);
            await player.EndTurn();
            Require(player.Health == healthBeforeShield - 5 && !player.HasEffect(StatusEffectType.Plague), "Plague deals damage on its final turn and expires");
            await player.Heal(5);
            await Capture("02-dungeon-entrance");
            await ClickChoice("Continue");
            await ClickChoice("Face");
            var opponent = ActionManager.Instance.SelectedTarget;
            var modifiers = opponent.PhysicalDamageModifiers.Count;
            await opponent.ApplyEffect(StatusEffectType.Frozen, 1);
            await opponent.EndTurn();
            Require(!opponent.HasEffect(StatusEffectType.Frozen) && opponent.PhysicalDamageModifiers.Count == modifiers, "Frozen weakness is removed on expiry");
            await Capture("03-spell-combat");
            ResourceTabs.CurrentTab = ResourceTabs.GetTabCount() - 1;
            await Capture("07-grid-map");
            ResourceTabs.CurrentTab = 0;
            await CheckMouseSelection();
            await WinBattle();
            Require(SceneManager.Instance.ChoosingReward, "Victory opens rewards");
            await Capture("04-reward-choice");
            var blessings = ManaSourceManager.Instance.BlessingBar.AllMana;
            await ClickChoice("Accept a");
            Require(ManaSourceManager.Instance.BlessingBar.AllMana == blessings + 1, "Reward increases mana capacity");
            await ClickChoice("Continue · Left");
            await ClickChoice("Drink");
            await ClickChoice("Continue");
            await ClickChoice("Rest");
            await ClickChoice("Explore the archive");
            await ClickChoice("Face");
            await WinBattle();
            await ClickChoice("Learn");
            Require(SpellBookManager.Instance.AvailableSpells.Count == 3, "Spell reward enters spellbook");
            await ClickChoice("Continue");
            await ClickChoice("Continue");
            await ClickChoice("Challenge");
            await WinBattle();
            Require(SceneManager.Instance.RunFinished && SceneManager.Instance.CurrentRoom.Id == "exit", "Boss victory finishes run");
            Require(SceneManager.Instance.ClearedRooms.Count == 3, "Both branches and boss are completable");
            await Capture("05-victory");
            TestStage = 1;
            await ClickChoice("Begin another");
        }
        catch (Exception error)
        {
            GD.PushError($"MVP CHECK FAILED: {error}");
            GetTree().Quit(1);
        }
    }

    private async Task CheckReplayAndDefeat()
    {
        var player = PlayerManager.Instance.GetPlayer();
        Require(!player.IsDead && player.Health == player.CharacterData.MaxHealth, "Replay restores health and life");
        Require(SpellBookManager.Instance.AvailableSpells.Count == 0 && ManaSourceManager.Instance.BlessingBar.AllMana == 0,
            "Replay clears spells and blessings");
        Require(SceneManager.Instance.CurrentRoom == null && SceneManager.Instance.ClearedRooms.Count == 0, "Replay clears dungeon progress");
        if (TestStage == 2)
        {
            await FinishChecks();
            return;
        }
        PlayerNameInput.Text = "Mart";
        Creation.OnSpinboxValueChanged();
        Creation.SubmitForm();
        await WaitUntil(() => Intro.Visible, "Replay intro opened");
        Intro._Input(new InputEventKey { Pressed = true, Keycode = Key.Space });
        await WaitUntil(() => SceneManager.Instance.CurrentRoom != null, "Replay dungeon entered");
        await ClickChoice("Continue");
        await ClickChoice("Face");
        var battle = BattleManager.Instance;
        for (var turn = 0; turn < 30 && battle.IsBattleActive; turn++)
        {
            await WaitUntil(() => battle.CanQueueSpells || !battle.IsBattleActive, "Defeat test player turn");
            if (battle.CanQueueSpells)
            {
                battle.StartNewTurnPhaseFrom(TurnPhase.Main);
            }
        }
        await WaitUntil(() => SceneManager.Instance.RunFinished, "Defeat screen shown");
        Require(player.IsDead && player.Health == 0, "Empty queues pass turns until defeat without negative health");
        await Capture("06-defeat");
        TestStage = 2;
        await ClickChoice("Begin another");
    }

    private async Task Capture(string name)
    {
        if (!OS.GetCmdlineUserArgs().Contains("--mvp-capture"))
        {
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute("res://.godot/mvp-preview");
        using var screenshot = GetViewport().GetTexture().GetImage();
        screenshot.SavePng($"res://.godot/mvp-preview/{name}.png");
    }

    /// <summary>
    /// Lets the audio mixer release active playback before the accelerated run exits.
    /// </summary>
    private async Task FinishChecks()
    {
        Music.Stop();
        Music.Stream = null;
        Engine.TimeScale = 1;
        await this.Delay(100);
        GD.Print("MVP INTEGRATION CHECKS PASSED (victory, defeat, replay)");
        GetTree().Quit(0);
    }

    private async Task WinBattle()
    {
        var battle = BattleManager.Instance;
        var player = PlayerManager.Instance.GetPlayer();
        for (var round = 0; round < 30 && battle.IsBattleActive; round++)
        {
            await WaitUntil(() => battle.CanQueueSpells || !battle.IsBattleActive, "Player turn");
            if (!battle.IsBattleActive)
            {
                break;
            }

            foreach (var spell in SpellBookManager.Instance.AvailableSpells.Where(s => s.Data.Name is "Light Pierce" or "Rot"))
            {
                var guard = 0;
                while (ManaSourceManager.Instance.CanPay(spell.Data.Cost) && guard++ < 10)
                {
                    ManaSourceManager.Instance.AutoselectMana(spell.Data.Cost);
                    ActionManager.Instance.SetSelectedSpell(spell);
                    AddSpell.OnPressed();
                }
            }
            Require(player.ActionQueue.Count > 0, "Player can queue affordable spells");
            battle.StartNewTurnPhaseFrom(TurnPhase.Main);
            battle.StartNewTurnPhaseFrom(TurnPhase.Main);
            Require(!battle.CanQueueSpells, "Casting locks input against duplicate clicks");
        }
        await WaitUntil(() => !battle.IsBattleActive, "Battle completed");
        Require(!player.IsDead, "Player survived encounter");
    }

    private async Task CheckMouseSelection()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var card = SpellList.GetChildren().OfType<SpellUI>().First();
        var point = card.GetGlobalRect().GetCenter();
        GetViewport().PushInput(new InputEventMouseMotion { Position = point }, true);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = true }, true);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = false }, true);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Require(ActionManager.Instance.SpellIsSelected, "Mouse click selects a spell through its child controls");
        Require(!AddSpell.Disabled, "Selected spell enables queue button");
        AddSpell.EmitSignal(Button.SignalName.Pressed);
        Require(PlayerManager.Instance.GetPlayer().ActionQueue.Count == 1, "Queue button adds selected spell");
        ClearQueue.EmitSignal(Button.SignalName.Pressed);
        Require(PlayerManager.Instance.GetPlayer().ActionQueue.Count == 0 && ManaSourceManager.Instance.BlessingBar.AllBlessings.All(b => b.State == State.Available), "Clearing queue refunds all blessings");
    }
}
