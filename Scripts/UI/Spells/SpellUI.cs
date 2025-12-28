using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

/// <summary>
/// SpellUI is a user interface element that displays information about a spell.
/// TODO: Add documentation for the properties.
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
	public Node StatusEffectsContainer { get; set; }

	[Export]
	public StyleBox SelectedStyle { get; set; }

	[Export]
	public StyleBox DefaultStyle { get; set; }

	[Export]
	public PackedScene StatusEffectIconScene { get; set; }

	private ActionData ActionData { get; set; }

	private bool IsSelected { get; set; }

	private System.Collections.Generic.Dictionary<DamageType, Color> DamageTypeColors = new()
	{
		{ DamageType.Physical, new Color(.1f, .1f, .1f, .25f) }, // Gray
		{ DamageType.Fire, new Color(1, 0, 0, .25f) }, // Red
		{ DamageType.Ice, new Color(0, 0, 1, .25f) }, // Blue
		{ DamageType.Lightning, new Color(1, 1, 0, .25f) }, // Yellow
		{ DamageType.Dark, new Color(0, 0, 0, .25f) }, // Black
		{ DamageType.Light, new Color(1, 1, 1, .25f) }, // White
		{ DamageType.Sanity, new Color(.5f, 0, .5f, .25f) }, // Purple
		{ DamageType.Disease, new Color(.5f, .25f, .25f, .25f) }, // Brown
	};

	public override void _Ready()
	{
		UpdateStyle();
		CallDeferred(nameof(InitializeCustomSignals));
	}

	public override void _ExitTree()
	{
		Managers.ActionManager.SpellSelected -= OnSpellSelected;
	}

	/// <summary>
	/// Hover and click handling for the spell UI.
	/// </summary>
	public override void _GuiInput(InputEvent @event)
	{
		// Show tooltip
		if (Managers.TooltipManager.IsTooltipVisible)
        {
            Managers.TooltipManager.UpdatePosition(GetGlobalMousePosition());
        }
        else
        {
            Managers.TooltipManager.Show(
                string.Empty,
                $"Cost: {ActionData.Cost}",
                GetGlobalMousePosition()
            );
        }

		// Handle mouse input for selecting the spell
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (Managers.ActionManager.SelectedSpell?.Data == ActionData)
            {
				// Already selected
                return;
            }

			if (!Managers.ManaSourceManager.CanPay(ActionData.Cost))
			{
				Managers.BattleLogManager.Log("You do not have enough mana to cast this spell.");
				return;
			}

			// Try to autoselect mana for the spell
			Managers.ManaSourceManager.AutoselectMana(ActionData.Cost);
			Managers.ActionManager.SetSelectedSpell(ActionData);
			_ = Managers.SoundEffectManager.PlayButtonClick();
		}
	}

	/// <summary>
	/// Sets up the SpellUI with the given ActionData to show the spell's information.
	/// </summary>
	public void Setup(ActionData actionData)
	{
		if (actionData == null)
		{
			GD.PrintErr("actionData is null");
			return;
		}

		ActionData = actionData;

		SpellName.Text = actionData.Name;
		SetSpellDescription(actionData.Description, actionData.Keywords);
		SpellIcon.Texture = actionData.Image;

		// Fill list of status effect icons
		foreach (var statusEffect in ActionData.StatusEffects)
		{
			var iconInstance = StatusEffectIconScene.Instantiate<StatusEffectIcon>();
			StatusEffectsContainer.AddChild(iconInstance);
			iconInstance.Setup(statusEffect);
		}
	}

	/// <summary>
	/// Wires up the custom signals for the spell UI.
	/// </summary>
	private void InitializeCustomSignals()
	{
		Managers.ActionManager.SpellSelected += OnSpellSelected;
        MouseExited += OnMouseExited;
	}

	/// <summary>
	/// Handles the selection of a spell by updating the UI.
	/// </summary>
	private void OnSpellSelected(string spellName)
	{
		IsSelected = ActionData.Name == spellName;
		UpdateStyle();
	}

	/// <summary>
    /// Handles mouse exit events to hide the tooltip.
    /// </summary>
    private void OnMouseExited()
    {
        Managers.TooltipManager.Hide();
    }

	/// <summary>
	/// Formats the spell description with keywords and sets it to the description label.
	/// </summary>
	private void SetSpellDescription(string description, Array<Keyword> keywords)
	{
        var keywordList = new List<string>();
		foreach (var keyword in keywords)
		{
			var keywordName = keyword.ToString();
			keywordList.Add($"[color=violet]{keywordName}[/color]");
		}

		if (keywordList.Count == 0)
		{
			SpellDescription.Text = description;
			return;
		}

        string fullDescription = string.Join(", ", keywordList) + " " + description;
        SpellDescription.Text = fullDescription;
	}

	/// <summary>
	/// Draws the gradient background for the spell UI based on the damage types of the spell.
	/// </summary>
	public override void _Draw()
	{
		var gradient = new Gradient();
		var offsets = new List<float>();
		var colors = new List<Color>();
		for (int i = 0; i < ActionData.DamageTypes.Length; i++)
		{
			var color = DamageTypeColors.TryGetValue(ActionData.DamageTypes[i], out var damageColor) ? damageColor : new Color(1, 0, 0);

			// Get the position of the color in the gradient
			float position = (float)i / (ActionData.DamageTypes.Length - 1f);
			offsets.Add(position);
			colors.Add(color);
		}

		// The AddPoint method cannot be used, since it seems to only add to a gradient with default colours.
		// I can't seem to remove the default colours either.
		gradient.Offsets = offsets.ToArray();
		gradient.Colors = colors.ToArray();

		var gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = gradient;

		DrawTextureRect(gradientTexture, new Rect2(Vector2.Zero, Size), false);
	}
	
	private void UpdateStyle()
	{
		AddThemeStyleboxOverride("panel", IsSelected ? SelectedStyle : DefaultStyle);
	}
}