using System;
using System.Linq;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using DungeonRPG.Characters;
using Godot;
using Priest.Managers;

/// <summary>
/// Opt-in integration playthrough and screenshot capture, independent of production UI setup.
/// </summary>
public partial class MvpPlaytest : Node
{
    private static int TestStage;
    private static int BaselineBossRounds { get; set; }
    private static float BaselineBossHealth { get; set; }
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
                CheckStartingMana(Builds.GetSingleDomainBuild(first, 3), first);
                foreach (var second in Enum.GetValues<Domain>().Where(d => d != first))
                {
                    Require(SpellBookManager.Instance.GetStartingSpellsByDomain([first, second]).Count >= 2, $"Starting spells for {first}/{second}");
                    CheckStartingMana(Builds.GetDualDomainBuild(first, second, 3), first);
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
            Require(ManaSourceManager.Instance.BlessingBar.AllMana == 3
                && ManaSourceManager.Instance.BlessingBar.MaxMana == 3, "Run starts with three mana and no spare capacity");
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
            Require(!HasChoice("Claim a rare"), "Ordinary victory offers no mana upgrade");
            await ClickChoice("Learn");
            Require(SpellBookManager.Instance.AvailableSpells.Count == 3, "Spell reward enters spellbook");
            Require(ManaSourceManager.Instance.BlessingBar.AllMana == 3, "Learning a spell does not increase mana");
            await ClickChoice("Continue · Left");
            Require(!HasChoice("Ask") && !HasChoice("Claim a rare"), "Shrine offers recovery without mana");
            await ClickChoice("Drink");
            await ClickChoice("Continue");
            await ClickChoice("Rest");
            await ClickChoice("Explore the archive");
            await ClickChoice("Face");
            await WinBattle();
            Require(HasChoice("Claim a rare"), "Archive offers the floor's rare mana upgrade");
            Require(!HasChoice("Learn Solidify"), "Rewards exclude spells outside the current domain budget");
            await Capture("08-rare-blessing-choice");
            await ClickChoice("Mend your wounds");
            Require(ManaSourceManager.Instance.BlessingBar.AllMana == 3
                && SceneManager.Instance.BlessingRewardsClaimed == 0, "Choosing recovery forfeits the mana upgrade");
            await ClickChoice("Continue");
            await ClickChoice("Continue");
            await ClickChoice("Challenge");
            await WinBattle();
            Require(SceneManager.Instance.RunFinished && SceneManager.Instance.CurrentRoom.Id == "exit", "Boss victory finishes run");
            Require(SceneManager.Instance.ClearedRooms.Count == 3, "Both branches and boss are completable");
            BaselineBossRounds = BattleManager.Instance.Round;
            BaselineBossHealth = player.Health;
            Require(ManaSourceManager.Instance.BlessingBar.AllMana == 3, "Boss can be defeated without a mana upgrade");
            Require(player.Health < player.CharacterData.MaxHealth / 3, "Starting mana produces a narrow victory against the boss");
            GD.Print($"MANA BASELINE: {BaselineBossRounds} boss rounds, {BaselineBossHealth:0} health remaining at 3 mana.");
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
        Require(SceneManager.Instance.BlessingRewardsClaimed == 0, "Replay resets the floor's blessing reward limit");
        if (TestStage == 3)
        {
            await FinishChecks();
            return;
        }
        if (TestStage == 1)
        {
            await CheckRareBlessingRun();
            return;
        }

        await StartReplayRun();
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
        TestStage = 3;
        await ClickChoice("Begin another");
    }

    private bool HasChoice(string prefix)
    {
        return Choices.GetChildren().OfType<Button>().Any(button => button.Text.StartsWith(prefix));
    }

    /// <summary>
    /// Every starting alignment must retain both a tight budget and castable initial spells.
    /// </summary>
    private void CheckStartingMana(Build build, Domain dominantDomain)
    {
        Require(build.Blessings.Sum(blessing => blessing.ManaAmount) == 3, $"Three starting mana for {string.Join("/", build.Alignment)}");
        Require(build.Blessings.Count(blessing => blessing.Domain == dominantDomain) >= 2, "Dominant domain receives the larger share");
        foreach (var spell in build.Spells)
        {
            Require(spell.Data.Cost.Costs.GroupBy(cost => cost.Type).All(costs =>
                costs.Sum(cost => cost.Amount) <= build.Blessings.Where(blessing => blessing.Domain == costs.Key)
                    .Sum(blessing => blessing.ManaAmount)), $"Starting mana can cast {spell.Data.Name}");
        }
    }

    private async Task StartReplayRun()
    {
        PlayerNameInput.Text = "Mart";
        Creation.OnSpinboxValueChanged();
        Creation.SubmitForm();
        await WaitUntil(() => Intro.Visible, "Replay intro opened");
        Intro._Input(new InputEventKey { Pressed = true, Keycode = Key.Space });
        await WaitUntil(() => SceneManager.Instance.CurrentRoom != null, "Replay dungeon entered");
    }

    /// <summary>
    /// Compares the earned upgrade with the baseline run and checks that it cannot be farmed.
    /// </summary>
    private async Task CheckRareBlessingRun()
    {
        await StartReplayRun();
        await ClickChoice("Continue");
        await ClickChoice("Face");
        await WinBattle();
        Require(!HasChoice("Claim a rare"), "Mana remains unavailable at the ordinary guardian");
        await ClickChoice("Learn");
        await ClickChoice("Continue · Right");
        await ClickChoice("Face");
        await WinBattle();
        await ClickChoice("Claim a rare");
        Require(ManaSourceManager.Instance.BlessingBar.AllMana == 4
            && ManaSourceManager.Instance.BlessingBar.MaxMana == 4
            && SceneManager.Instance.BlessingRewardsClaimed == 1, "Rare blessing adds exactly one recurring mana");
        Require(!HasChoice("Learn") && !HasChoice("Mend"), "Claiming mana forfeits the other archive rewards");
        await ClickChoice("Continue");
        SceneManager.Instance.EnterRoom("archive");
        Require(!SceneManager.Instance.ChoosingReward && !HasChoice("Claim a rare"), "Revisiting the archive cannot grant another blessing");
        Require(ManaSourceManager.Instance.BlessingBar.AllMana == 4, "Mana remains capped after revisiting");
        await ClickChoice("Continue");
        await ClickChoice("Rest");
        await ClickChoice("Continue");
        await ClickChoice("Challenge");
        await WinBattle();
        var player = PlayerManager.Instance.GetPlayer();
        Require(SceneManager.Instance.RunFinished, "Rare blessing run reaches victory");
        Require(BattleManager.Instance.Round < BaselineBossRounds && player.Health > BaselineBossHealth,
            "One extra mana wins faster and saves health against the same boss");
        GD.Print($"MANA UPGRADE: {BattleManager.Instance.Round} boss rounds, {player.Health:0} health remaining at 4 mana.");
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
