using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using Godot;
using Priest.Managers;

/// <summary>
/// Coordinates room interactions, one-time rewards, and encounter transitions for a run.
/// </summary>
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    [Export]

    private DungeonFloorData Floor;
    [Export]
    private ComponentExposer ExplorationExposer;
    [Export]
    private CanvasItem BattleHud;
    [Export]
    private Label CombatHelp;
    [Export]
    private PackedScene ChoiceButtonScene;

    public DungeonGenerator Dungeon { get; private set; }
    public DungeonRoomData CurrentRoom { get; private set; }
    public HashSet<string> ClearedRooms { get; } = [];
    public HashSet<string> VisitedRooms { get; } = [];
    public bool RunFinished { get; private set; }
    public bool ChoosingReward { get; private set; }
    public bool IsResolvingChoice { get; private set; }

    public int BlessingRewardsClaimed { get; private set; }

    private bool CanOfferBlessingReward => CurrentRoom.OffersBlessingReward
        && BlessingRewardsClaimed < Floor.MaximumBlessingRewards;

    /// <summary>
    /// Notifies map views after room progress or a one-time interaction changes.
    /// </summary>
    [Signal]
    public delegate void MapChangedEventHandler();

    private HashSet<string> UsedEvents { get; } = [];
    private RichTextLabel Title { get; set; }
    private RichTextLabel Text { get; set; }
    private VBoxContainer Choices { get; set; }
    private Label RunStatus { get; set; }
    private BattleManager Battle { get; set; }

    public override void _Ready()
    {
        Instance = this;
        if (Floor == null || ExplorationExposer == null || BattleHud == null
            || CombatHelp == null || ChoiceButtonScene == null)
        {
            GD.PrintErr("SceneManager requires floor content, screen components, and a choice scene.");
            return;
        }

        InitializeComponents();
        InitializeCustomSignals();
    }

    private void InitializeComponents()
    {
        Title = ExplorationExposer.GetComponent<RichTextLabel>(Components.SceneTitle);
        Text = ExplorationExposer.GetComponent<RichTextLabel>(Components.SceneText);
        Choices = ExplorationExposer.GetComponent<VBoxContainer>(Components.SceneChoices);
        RunStatus = ExplorationExposer.GetComponent<Label>(Components.RunStatus);
    }

    private void InitializeCustomSignals()
    {
        Battle = BattleManager.Instance;
        Battle.BattleEnded += OnBattleEnded;
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(Battle))
        {
            Battle.BattleEnded -= OnBattleEnded;
        }
    }

    public bool IsRoomComplete(string id)
    {
        return ClearedRooms.Contains(id) || UsedEvents.Contains(id);
    }

    public void StartDungeon()
    {
        Dungeon = new DungeonGenerator(Floor);
        EnterRoom(Dungeon.Generate().Id);
    }

    /// <summary>
    /// Enters an adjacent room when the player is free to travel.
    /// </summary>
    public void EnterRoom(string id)
    {
        if (RunFinished || Battle.IsBattleActive || ChoosingReward)
        {
            return;
        }

        if (!Dungeon.Rooms.TryGetValue(id, out var room))
        {
            GD.PrintErr($"Unknown dungeon room: {id}");
            return;
        }

        if (CurrentRoom != null && CurrentRoom.Id != id
            && !CurrentRoom.Exits.Contains(id) && !CurrentRoom.OptionalBranches.Contains(id))
        {
            return;
        }

        CurrentRoom = room;
        VisitedRooms.Add(id);
        ShowRoom();
    }

    private void ShowRoom()
    {
        ChoosingReward = false;
        SetScene(new Scene { Title = CurrentRoom.Title, Text = CurrentRoom.Text });
        UpdateStatus();

        if (CurrentRoom.Kind == RoomKind.Exit)
        {
            RunFinished = true;
            AddChoice(Floor.FormatText("restart"), Restart);
            return;
        }

        if (CurrentRoom.HasEncounter && !ClearedRooms.Contains(CurrentRoom.Id))
        {
            var key = CurrentRoom.Kind == RoomKind.Boss ? "challenge" : "face";
            AddChoice(Floor.FormatText(key), BeginEncounter);
            return;
        }

        AddEventChoices();
        AddTravelChoices();
    }

    private void AddEventChoices()
    {
        if (UsedEvents.Contains(CurrentRoom.Id))
        {
            return;
        }

        if (CurrentRoom.Healing > 0)
        {
            var key = CurrentRoom.Kind == RoomKind.Shrine ? "shrine_heal" : "lantern_heal";
            AddAsyncChoice(Floor.FormatText(key, CurrentRoom.Healing), UseHealingEvent);
        }
    }

    private void AddTravelChoices()
    {
        foreach (var id in CurrentRoom.OptionalBranches)
        {
            if (!IsRoomComplete(id))
            {
                AddChoice(Floor.FormatText("explore", id), () => EnterRoom(id));
            }
        }

        foreach (var id in CurrentRoom.Exits)
        {
            AddChoice(Floor.FormatText("continue", Dungeon.Rooms[id].Title), () => EnterRoom(id));
        }
    }

    private async Task UseHealingEvent()
    {
        UsedEvents.Add(CurrentRoom.Id);
        await PlayerManager.Instance.GetPlayer().Heal(CurrentRoom.Healing);
        ShowRoom();
    }

    /// <summary>
    /// Starts the current room's configured encounter once, preserving the player's run state.
    /// </summary>
    public void BeginEncounter()
    {
        if (Battle.IsBattleActive || RunFinished || CurrentRoom?.Opponent == null
            || ClearedRooms.Contains(CurrentRoom.Id))
        {
            return;
        }

        ExplorationExposer.Hide();
        BattleHud.Show();
        TooltipManager.Instance.Hide();
        Battle.InitializeBattle((CharacterData)CurrentRoom.Opponent.Duplicate());
        UpdateCombatHelp();
    }

    private void OnBattleEnded(bool victory)
    {
        BattleHud.Hide();
        TooltipManager.Instance.Hide();

        if (!victory)
        {
            RunFinished = true;
            SetScene(new Scene
            {
                Title = Floor.FormatText("defeat_title"),
                Text = Floor.FormatText("defeat_text")
            });
            AddChoice(Floor.FormatText("restart"), Restart);
            return;
        }

        ClearedRooms.Add(CurrentRoom.Id);
        if (CurrentRoom.Kind == RoomKind.Boss)
        {
            EnterRoom(CurrentRoom.Exits[0]);
        }
        else
        {
            ShowRewards();
        }
    }

    private void ShowRewards()
    {
        ChoosingReward = true;
        var description = Floor.FormatText("reward_text");
        if (CanOfferBlessingReward)
        {
            description += "\n\n" + Floor.FormatText("rare_blessing_text");
        }

        SetScene(new Scene
        {
            Title = Floor.FormatText("reward_title"),
            Text = description
        });

        var domains = ManaSourceManager.Instance.BlessingBar.AllBlessings
            .Select(blessing => blessing.Domain).Distinct().ToArray();
        AddSpellRewards(domains);
        if (CanOfferBlessingReward)
        {
            foreach (var domain in domains)
            {
                AddChoice(Floor.FormatText("reward_blessing", domain), () => AcceptBlessingReward(domain));
            }
        }

        AddAsyncChoice(Floor.FormatText("reward_heal", Floor.RewardHealing), AcceptHealingReward);
        UpdateStatus();
    }

    private void AddSpellRewards(Domain[] domains)
    {
        foreach (var spell in Floor.RewardSpells)
        {
            spell.Initialize();
            if (SpellBookManager.Instance.AvailableSpells.Any(known => known.Data.Name == spell.Name)
                || spell.Cost.Costs.Any(cost => !domains.Contains(cost.Type)))
            {
                continue;
            }

            // A reward must be castable with the player's present mana budget.
            if (spell.Cost.Costs.GroupBy(cost => cost.Type).Any(costs =>
                costs.Sum(cost => cost.Amount) > ManaSourceManager.Instance.BlessingBar.AllBlessings
                    .Where(blessing => blessing.Domain == costs.Key).Sum(blessing => blessing.ManaAmount)))
            {
                continue;
            }

            AddChoice(Floor.FormatText("learn", spell.Name, spell.Cost), () => AcceptSpellReward(spell));
        }
    }

    private void AcceptSpellReward(ActionData spell)
    {
        SpellBookManager.Instance.AddSpell(spell);
        ShowRoom();
    }

    private void AcceptBlessingReward(Domain domain)
    {
        if (!ChoosingReward || !CanOfferBlessingReward)
        {
            return;
        }

        GrantBlessing(domain);
        BlessingRewardsClaimed++;
        ShowRoom();
    }

    private async Task AcceptHealingReward()
    {
        await PlayerManager.Instance.GetPlayer().Heal(Floor.RewardHealing);
        ShowRoom();
    }

    private void GrantBlessing(Domain domain)
    {
        var mana = ManaSourceManager.Instance;
        mana.BlessingBar.MaxMana++;
        mana.AddBlessing(new Blessing(Level.Minor, domain));
    }

    public void SetScene(Scene scene)
    {
        BattleHud.Hide();
        ExplorationExposer.Show();
        Title.Text = scene.Title;
        Text.Text = scene.Text;

        foreach (var child in Choices.GetChildren())
        {
            Choices.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var choice in scene.Choices)
        {
            AddChoice(choice.Title, choice.Effect, choice.Prerequisite?.Invoke() ?? true);
        }
    }

    private void AddChoice(string title, System.Action effect, bool enabled = true)
    {
        AddAsyncChoice(title, () =>
        {
            effect?.Invoke();
            return Task.CompletedTask;
        }, enabled);
    }

    private void AddAsyncChoice(string title, Func<Task> effect, bool enabled = true)
    {
        var button = ChoiceButtonScene.Instantiate<SceneChoiceButton>();
        button.Setup(title, effect, enabled);
        Choices.AddChild(button);
    }

    /// <summary>
    /// Locks all room choices until the selected action finishes updating the view.
    /// </summary>
    public async Task ResolveChoice(Func<Task> effect)
    {
        if (IsResolvingChoice)
        {
            return;
        }

        IsResolvingChoice = true;
        foreach (var child in Choices.GetChildren().OfType<Button>())
        {
            child.Disabled = true;
        }

        try
        {
            await effect();
        }
        finally
        {
            IsResolvingChoice = false;
        }
    }

    private void UpdateStatus()
    {
        var player = PlayerManager.Instance.GetPlayer();
        RunStatus.Text = Floor.FormatText("status", player.CharacterData.Name, player.Health,
            player.CharacterData.MaxHealth, ManaSourceManager.Instance.BlessingBar.AllMana, ClearedRooms.Count);
        EmitSignal(SignalName.MapChanged);
    }

    private void UpdateCombatHelp()
    {
        var enemy = Battle.Characters.Keys.OfType<Enemy>().First();
        CombatHelp.Text = Floor.FormatText("combat_help", CurrentRoom.Title, enemy.GetResistanceSummary());
    }

    private void Restart()
    {
        GetTree().ReloadCurrentScene();
    }
}
