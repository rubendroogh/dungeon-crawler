using System;
using System.Linq;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using Godot;
using Priest.Managers;

/// <summary>Viewport setup and opt-in integration checks (-- --mvp-test).</summary>
public partial class MvpPlaytest : Node
{
    private static int TestStage;
    public override void _Ready()
    {
        try { ConfigureInterface(); }
        catch (Exception error)
        {
            GD.PushError($"Game setup failed: {error}");
            if (OS.GetCmdlineUserArgs().Contains("--mvp-test")) GetTree().Quit(1);
        }
    }

    private void ConfigureInterface()
    {
        var ui = GetNode<Control>("UI");
        // Keep the established Root/UI paths; use viewport coordinates for the interface.
        GetNode<Camera2D>("World/Camera").Enabled = false;
        ui.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        GetNode<Control>("UI/BattleHUD").SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var blessingBar = GetNode("UI/BattleHUD").FindChild("BlessingBar");
        blessingBar.Set("nine_patch_stretch", true);
        blessingBar.Set("custom_minimum_size", new Vector2(0, 48));
        blessingBar.Set("size_flags_horizontal", (int)Control.SizeFlags.ExpandFill);
        foreach (var texture in blessingBar.GetChildren().OfType<TextureRect>())
        {
            texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            texture.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            texture.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        var creation = GetNode<Control>("UI/CharacterCreation");
        creation.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        foreach (var control in creation.GetChildren().OfType<Control>())
        {
            control.Scale = Vector2.One;
            control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        }
        var cutscene = GetNode<Control>("UI/IntroCutscene");
        cutscene.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        cutscene.GetNode<Control>("CenterContainer").SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var background = new DungeonBackdrop { Name = "DungeonBackdrop", ZIndex = -5 };
        ui.AddChild(background);
        ui.MoveChild(background, 0);
        background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        background.FollowCombatArea(GetNode<Control>("UI/BattleHUD"), GetNode("UI/BattleHUD/BattleAndResources/Battle/Opponents"));
        var panel = new StyleBoxFlat { BgColor = new Color(.035f, .045f, .065f, .94f), ContentMarginLeft = 36, ContentMarginRight = 36, ContentMarginTop = 24, ContentMarginBottom = 24 };
        var sceneColumn = GetNode("UI/Scene").FindChild("ContentMarginContainer") as Control;
        var scenePanel = new PanelContainer { ZIndex = -1, MouseFilter = Control.MouseFilterEnum.Ignore };
        scenePanel.AddThemeStyleboxOverride("panel", panel);
        sceneColumn.AddChild(scenePanel);
        scenePanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        sceneColumn.GetParent<Control>().CustomMinimumSize = new Vector2(1000, 0);
        foreach (var edge in new[] { "left", "right", "top", "bottom" })
            sceneColumn.AddThemeConstantOverride("margin_" + edge, 28);
        var queue = GetNode("UI/BattleHUD").FindChild("SpellQueueList");
        var clear = new Button { Name = "ClearQueue", Text = "Clear queue · return blessings" };
        clear.Pressed += () =>
        {
            if (!BattleManager.Instance.CanQueueSpells) return;
            PlayerManager.Instance.GetPlayer().ActionQueue.Clear();
            ActionManager.Instance.ClearSelectedSpell();
            ManaSourceManager.Instance.ResetAllMana();
            SpellQueueManager.Instance.UpdateSpellQueue();
        };
        queue.GetParent().AddChild(clear);
        if (OS.GetCmdlineUserArgs().Contains("--mvp-test")) CallDeferred(nameof(RunChecks));
    }

    private void Require(bool value, string description)
    {
        if (!value) throw new InvalidOperationException(description);
        GD.Print($"PASS: {description}");
    }

    private async Task WaitUntil(Func<bool> condition, string description)
    {
        for (var i = 0; i < 5000; i++)
        {
            if (condition()) return;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        throw new TimeoutException(description);
    }

    private async Task ClickChoice(string prefix)
    {
        await WaitUntil(() => !SceneManager.Instance.IsResolvingChoice, "Previous choice completed");
        var choices = GetNode("UI/Scene").FindChild("SceneChoices");
        var button = choices.GetChildren().OfType<Button>().First(b => b.Text.StartsWith(prefix));
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
                    Require(SpellBookManager.Instance.GetStartingSpellsByDomain([first, second]).Count >= 2, $"Starting spells for {first}/{second}");
            }
            var creation = GetNode("UI/CharacterCreation").FindChild("CharacterCreationManager") as CharacterCreationManager;
            (creation.FindChild("PlayerNameInput") as LineEdit).Text = "Mart";
            creation.OnSpinboxValueChanged();
            Require(!(creation.FindChild("SubmitButton") as Button).Disabled, "Character form is valid");
            creation.SubmitForm();
            await WaitUntil(() => GetNode<Control>("UI/IntroCutscene").Visible, "Intro opened");
            var intro = GetNode<IntroCutscene>("UI/IntroCutscene");
            intro._Input(new InputEventKey { Pressed = true, Keycode = Key.Space });
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
            GD.Print("MVP INTEGRATION CHECKS PASSED (victory, defeat, replay)");
            GetTree().Quit(0);
            return;
        }
        var creation = GetNode("UI/CharacterCreation").FindChild("CharacterCreationManager") as CharacterCreationManager;
        (creation.FindChild("PlayerNameInput") as LineEdit).Text = "Mart";
        creation.OnSpinboxValueChanged();
        creation.SubmitForm();
        await WaitUntil(() => GetNode<Control>("UI/IntroCutscene").Visible, "Replay intro opened");
        GetNode<IntroCutscene>("UI/IntroCutscene")._Input(new InputEventKey { Pressed = true, Keycode = Key.Space });
        await WaitUntil(() => SceneManager.Instance.CurrentRoom != null, "Replay dungeon entered");
        await ClickChoice("Continue");
        await ClickChoice("Face");
        var battle = BattleManager.Instance;
        for (var turn = 0; turn < 30 && battle.IsBattleActive; turn++)
        {
            await WaitUntil(() => battle.CanQueueSpells || !battle.IsBattleActive, "Defeat test player turn");
            if (battle.CanQueueSpells) battle.StartNewTurnPhaseFrom(TurnPhase.Main);
        }
        await WaitUntil(() => SceneManager.Instance.RunFinished, "Defeat screen shown");
        Require(player.IsDead && player.Health == 0, "Empty queues pass turns until defeat without negative health");
        await Capture("06-defeat");
        TestStage = 2;
        await ClickChoice("Begin another");
    }

    private async Task Capture(string name)
    {
        if (!OS.GetCmdlineUserArgs().Contains("--mvp-capture")) return;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute("res://.godot/mvp-preview");
        GetViewport().GetTexture().GetImage().SavePng($"res://.godot/mvp-preview/{name}.png");
    }

    private async Task WinBattle()
    {
        var battle = BattleManager.Instance;
        var player = PlayerManager.Instance.GetPlayer();
        for (var round = 0; round < 30 && battle.IsBattleActive; round++)
        {
            await WaitUntil(() => battle.CanQueueSpells || !battle.IsBattleActive, "Player turn");
            if (!battle.IsBattleActive) break;
            var add = GetNode("UI/BattleHUD").FindChild("AddSpellToQueue") as AddSpellToQueueButton;
            foreach (var spell in SpellBookManager.Instance.AvailableSpells.Where(s => s.Data.Name is "Light Pierce" or "Rot"))
            {
                var guard = 0;
                while (ManaSourceManager.Instance.CanPay(spell.Data.Cost) && guard++ < 10)
                {
                    ManaSourceManager.Instance.AutoselectMana(spell.Data.Cost);
                    ActionManager.Instance.SetSelectedSpell(spell);
                    add.OnPressed();
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
        var card = GetNode("UI/BattleHUD").FindChild("SpellBookList").GetChildren().OfType<SpellUI>().First();
        var point = card.GetGlobalRect().GetCenter();
        GetViewport().PushInput(new InputEventMouseMotion { Position = point }, true);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = true }, true);
        GetViewport().PushInput(new InputEventMouseButton { Position = point, ButtonIndex = MouseButton.Left, Pressed = false }, true);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Require(ActionManager.Instance.SpellIsSelected, "Mouse click selects a spell through its child controls");
        var add = GetNode("UI/BattleHUD").FindChild("AddSpellToQueue") as Button;
        Require(!add.Disabled, "Selected spell enables queue button");
        add.EmitSignal(Button.SignalName.Pressed);
        Require(PlayerManager.Instance.GetPlayer().ActionQueue.Count == 1, "Queue button adds selected spell");
        (GetNode("UI/BattleHUD").FindChild("ClearQueue", true, false) as Button).EmitSignal(Button.SignalName.Pressed);
        Require(PlayerManager.Instance.GetPlayer().ActionQueue.Count == 0 && ManaSourceManager.Instance.BlessingBar.AllBlessings.All(b => b.State == State.Available), "Clearing queue refunds all blessings");
    }
}

