using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonRPG.Blessings.Enums;
using Godot;
using Priest.Managers;

/// <summary>Runs one branching descent, including encounters, choices and one-time rewards.</summary>
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    public DungeonGenerator Dungeon { get; private set; }
    public DungeonRoom CurrentRoom { get; private set; }
    public HashSet<string> ClearedRooms { get; } = new();
    public HashSet<string> VisitedRooms { get; } = new();
    public bool RunFinished { get; private set; }
    public bool ChoosingReward { get; private set; }
    private readonly HashSet<string> UsedEvents = new();
    private CanvasItem SceneNode;
    private CanvasItem BattleNode;
    private RichTextLabel Title;
    private RichTextLabel Text;
    private VBoxContainer Choices;
    private Label RunStatus;
    private Label CombatHelp;
    public event System.Action MapChanged;
    public bool IsRoomComplete(string id) => ClearedRooms.Contains(id) || UsedEvents.Contains(id);
    private bool ChoiceInProgress;
    public bool IsResolvingChoice => ChoiceInProgress;

    public override void _Ready()
    {
        Instance = this;
        SceneNode = GetTree().Root.GetNode<CanvasItem>("Root/UI/Scene");
        BattleNode = GetTree().Root.GetNode<CanvasItem>("Root/UI/BattleHUD");
        Title = SceneNode.FindChild("SceneTitle") as RichTextLabel;
        Text = SceneNode.FindChild("SceneText") as RichTextLabel;
        Choices = SceneNode.FindChild("SceneChoices") as VBoxContainer;
        Text.CustomMinimumSize = new Vector2(900, 0);
        Text.AddThemeFontSizeOverride("normal_font_size", 24);
        Text.BbcodeEnabled = false;
        var column = Title.GetParent();
        RunStatus = new Label { Name = "RunStatus" };
        column.AddChild(RunStatus);
        column.MoveChild(RunStatus, 0);
        RunStatus.AddThemeColorOverride("font_color", new Color("c9b786"));
        var mapTab = BattleNode.FindChild("Map") as Control;
        foreach (var child in mapTab.GetChildren()) { mapTab.RemoveChild(child); child.QueueFree(); }
        var combatMap = new DungeonMap();
        mapTab.AddChild(combatMap);
        combatMap.Bind(this);
        var explorationPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(400, 800),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
        };
        (SceneNode as Control).AddThemeConstantOverride("separation", 28);
        SceneNode.AddChild(explorationPanel);
        var explorationMap = new DungeonMap();
        explorationPanel.AddChild(explorationMap);
        explorationMap.Bind(this);
        var inventory = BattleNode.FindChild("Inventory") as Control;
        if (inventory != null) inventory.GetParent<TabContainer>().SetTabHidden(inventory.GetIndex(), true);
        CombatHelp = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(600, 100) };
        CombatHelp.AddThemeConstantOverride("outline_size", 4);
        CombatHelp.AddThemeColorOverride("font_outline_color", Colors.Black);
        var combatColumn = BattleNode.GetNode("BattleAndResources/Battle");
        combatColumn.AddChild(CombatHelp);
        combatColumn.MoveChild(CombatHelp, 0);
        BattleManager.Instance.BattleEnded += OnBattleEnded;
    }

    public void StartDungeon()
    {
        Dungeon = new DungeonGenerator();
        EnterRoom(Dungeon.Generate().Id);
    }

    public void EnterRoom(string id)
    {
        if (RunFinished || BattleManager.Instance.IsBattleActive || ChoosingReward) return;
        if (CurrentRoom != null && !CurrentRoom.Exits.Contains(id) && id != CurrentRoom.Id
            && !(CurrentRoom.Id == "crossroads" && (id == "shrine" || id == "archive"))) return;
        CurrentRoom = Dungeon.Rooms[id];
        VisitedRooms.Add(id);
        ShowRoom();
    }

    private void ShowRoom()
    {
        ChoosingReward = false;
        var room = CurrentRoom;
        SetScene(new Scene { Title = room.Title, Text = room.Text });
        UpdateStatus();
        if (room.Kind == RoomKind.Exit)
        {
            RunFinished = true;
            AddChoice("Begin another descent", Restart);
            return;
        }
        if ((room.Kind == RoomKind.Battle || room.Kind == RoomKind.Archive || room.Kind == RoomKind.Boss) && !ClearedRooms.Contains(room.Id))
        {
            AddChoice(room.Kind == RoomKind.Boss ? "Challenge the Keeper" : "Face the guardian", BeginEncounter);
            return;
        }
        if (room.Kind == RoomKind.Shrine && !UsedEvents.Contains(room.Id))
        {
            AddAsyncChoice("Drink from the basin · recover 24 health", async () =>
            {
                UsedEvents.Add(room.Id);
                await PlayerManager.Instance.GetPlayer().Heal(24);
                ShowRoom();
            });
            foreach (var domain in Enum.GetValues<Domain>())
            {
                var chosen = domain;
                AddChoice($"Ask {domain} for a blessing · +1 mana each turn", () =>
                {
                    UsedEvents.Add(room.Id);
                    GrantBlessing(chosen);
                    ShowRoom();
                });
            }
        }
        if (room.Kind == RoomKind.Crossroads && !UsedEvents.Contains(room.Id))
        {
            AddAsyncChoice("Rest beside the lantern · recover 18 health", async () =>
            {
                UsedEvents.Add(room.Id);
                await PlayerManager.Instance.GetPlayer().Heal(18);
                ShowRoom();
            });
        }
        if (room.Kind == RoomKind.Crossroads)
        {
            foreach (var branch in new[] { "shrine", "archive" })
            {
                var destination = branch;
                if (!UsedEvents.Contains(branch) && !ClearedRooms.Contains(branch))
                    AddChoice($"Explore the {(branch == "shrine" ? "shrine" : "archive")}", () => EnterRoom(destination));
            }
        }
        foreach (var exit in room.Exits)
        {
            var destination = exit;
            AddChoice($"Continue · {Dungeon.Rooms[exit].Title}", () => EnterRoom(destination));
        }
    }

    public void BeginEncounter()
    {
        if (BattleManager.Instance.IsBattleActive || RunFinished || ClearedRooms.Contains(CurrentRoom.Id)) return;
        SceneNode.Hide();
        BattleNode.Show();
        TooltipManager.Instance.Hide();
        BattleManager.Instance.InitializeBattle(CreateOpponent(CurrentRoom.Kind));
        UpdateCombatHelp();
    }

    public static CharacterData CreateOpponent(RoomKind kind)
    {
        var data = (CharacterData)GD.Load<CharacterData>("res://Resources/Characters/BasicEnemy.tres").Duplicate();
        data.Name = kind == RoomKind.Boss ? "Keeper of the Influx" : kind == RoomKind.Archive ? "Drowned Archivist" : "Undead Squid";
        data.MaxHealth = kind == RoomKind.Boss ? 100 : kind == RoomKind.Archive ? 52 : 36;
        data.BaseFireDamageMultiplier = kind == RoomKind.Boss ? 0.75f : 1;
        data.BaseLightDamageMultiplier = 1.25f;
        data.BaseIceDamageMultiplier = kind == RoomKind.Boss ? 1.25f : 1;
        data.BaseDiseaseDamageMultiplier = 1;
        data.Image = new AtlasTexture
        {
            Atlas = GD.Load<Texture2D>("res://Assets/Textures/Atlasses/CreaturesTileMap.png"),
            Region = new Rect2(kind == RoomKind.Boss ? 192 : kind == RoomKind.Archive ? 64 : 0, 0, 32, 32)
        };
        return data;
    }

    private void OnBattleEnded(bool victory)
    {
        BattleNode.Hide();
        TooltipManager.Instance.Hide();
        if (!victory)
        {
            RunFinished = true;
            SetScene(new Scene { Title = "The dungeon remembers", Text = "Your blessings fade beneath Alderwick. The old world's secret remains beyond your reach.\n\nTry another personality, change the order of your spells, and watch the enemy's next attack." });
            AddChoice("Begin another descent", Restart);
            return;
        }
        ClearedRooms.Add(CurrentRoom.Id);
        if (CurrentRoom.Kind == RoomKind.Boss) EnterRoom("exit");
        else ShowRewards();
    }

    private void ShowRewards()
    {
        ChoosingReward = true;
        SetScene(new Scene { Title = "A gift from the remnants", Text = "The guardian falls. The gods offer a single gift for your descent.\n\nChoose one reward. Your blessings refresh at the start of every turn; your health carries between encounters." });
        var mana = ManaSourceManager.Instance;
        var domains = mana.BlessingBar.AllBlessings.Select(b => b.Domain).Distinct().ToArray();
        var paths = new[] { "Damage/FireBall", "Damage/FileBolt", "Damage/LightPierce", "Damage/Rot", "Damage/Solidify", "Damage/Discombobulate", "StatusEffect/Plague", "Buff/UmbralShield", "QueueSynergy/Duplication", "QueueSynergy/Echo" };
        foreach (var path in paths)
        {
            var spell = GD.Load<ActionData>($"res://Resources/Spells/{path}.tres");
            spell.Initialize();
            if (SpellBookManager.Instance.AvailableSpells.Any(s => s.Data.Name == spell.Name)
                || spell.Cost.Costs.Any(c => !domains.Contains(c.Type))) continue;
            AddChoice($"Learn {spell.Name} · {spell.Cost}", () =>
            {
                SpellBookManager.Instance.AddSpell(spell);
                ShowRoom();
            });
        }
        foreach (var domain in domains)
        {
            var chosen = domain;
            AddChoice($"Accept a {domain} blessing · +1 mana each turn", () => { GrantBlessing(chosen); ShowRoom(); });
        }
        AddAsyncChoice("Mend your wounds · recover 22 health", async () =>
        {
            await PlayerManager.Instance.GetPlayer().Heal(22);
            ShowRoom();
        });
        UpdateStatus();
    }

    private void GrantBlessing(Domain domain)
    {
        var mana = ManaSourceManager.Instance;
        mana.BlessingBar.MaxMana++;
        mana.AddBlessing(new Blessing(Level.Minor, domain));
    }

    public void SetScene(Scene scene)
    {
        BattleNode.Hide();
        SceneNode.Show();
        Title.Text = scene.Title;
        Text.Text = scene.Text;
        foreach (var child in Choices.GetChildren()) { Choices.RemoveChild(child); child.QueueFree(); }
        foreach (var choice in scene.Choices)
            AddChoice(choice.Title, choice.Effect, choice.Prerequisite?.Invoke() ?? true);
    }

    private void AddChoice(string title, System.Action effect, bool enabled = true)
        => AddAsyncChoice(title, () => { effect?.Invoke(); return Task.CompletedTask; }, enabled);

    private void AddAsyncChoice(string title, Func<Task> effect, bool enabled = true)
    {
        var button = new Button { Text = title, Disabled = !enabled, CustomMinimumSize = new Vector2(900, 52), Alignment = HorizontalAlignment.Left, MouseDefaultCursorShape = Control.CursorShape.PointingHand };
        button.Pressed += async () =>
        {
            if (ChoiceInProgress || !IsInstanceValid(button) || button.IsQueuedForDeletion()) return;
            ChoiceInProgress = true;
            foreach (var child in Choices.GetChildren().OfType<Button>()) child.Disabled = true;
            try { await effect(); }
            finally { ChoiceInProgress = false; }
        };
        Choices.AddChild(button);
    }

    private void UpdateStatus()
    {
        var player = PlayerManager.Instance.GetPlayer();
        RunStatus.Text = $"PRIEST  /  ALDERWICK · FIRST DESCENT\n{player.CharacterData.Name} · Health {player.Health:0}/{player.CharacterData.MaxHealth:0} · Blessings {ManaSourceManager.Instance.BlessingBar.AllMana} · Guardians defeated {ClearedRooms.Count}/3\n";
        MapChanged?.Invoke();
    }

    private void UpdateCombatHelp()
    {
        var enemy = BattleManager.Instance.Characters.Keys.OfType<Enemy>().First();
        CombatHelp.Text = $"{CurrentRoom.Title}\n{enemy.GetResistanceSummary()}\nSelect a spell → Add to queue → Cast queue. Blessings refresh each turn. Order matters!";
    }

    private void Restart() => GetTree().ReloadCurrentScene();
}
