using Godot;

/// <summary>
/// SpellUI is a user interface element that displays information about a spell.
/// </summary>
public partial class SpellUI : PanelContainer
{
	[Export]
	public RichTextLabel SpellName { get; set; }

	[Export]
	public RichTextLabel SpellDescription { get; set; }

	[Export]
	public TextureRect SpellIcon { get; set; }

	[Export]
	public StyleBox SelectedStyle { get; set; }

	[Export]
	public StyleBox DefaultStyle { get; set; }

	private SpellData SpellData { get; set; }

	private bool IsSelected { get; set; }

	public override void _Ready()
	{
		CallDeferred(nameof(InitializeCustomSignals));
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			ManagerRepository.SpellCastingManager.SetSelectedSpell(SpellData);
		}
	}

	public void Setup(SpellData spellData)
	{
		if (spellData == null)
		{
			GD.PrintErr("SpellData is null");
			return;
		}

		SpellData = spellData;

		SpellName.Text = spellData.Name;
		SpellDescription.Text = spellData.Description;
		SpellIcon.Texture = spellData.Image;
	}

	private void InitializeCustomSignals()
	{
		var spellCastingManager = ManagerRepository.SpellCastingManager;
		spellCastingManager.SpellSelected += OnSpellSelected;
	}

	private void OnSpellSelected(string spellName)
	{
		IsSelected = SpellData.Name == spellName;
		UpdateStyle();
	}
	
	private void UpdateStyle()
	{
		AddThemeStyleboxOverride("panel", IsSelected ? SelectedStyle : DefaultStyle);
	}
}
