using System.Collections.Generic;
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

	private ActionData ActionData { get; set; }

	private bool IsSelected { get; set; }

	private Dictionary<DamageType, Color> DamageTypeColors = new()
	{
		{ DamageType.Physical, new Color(.1f, .1f, .1f, .1f) }, // Gray
		{ DamageType.Fire, new Color(1, 0, 0, .1f) }, // Red
		{ DamageType.Ice, new Color(0, 0, 1, .1f) }, // Blue
		{ DamageType.Lightning, new Color(1, 1, 0, .1f) }, // Yellow
		{ DamageType.Dark, new Color(0, 0, 0, .1f) }, // Black
		{ DamageType.Light, new Color(1, 1, 1, .1f) }, // White
		{ DamageType.Sanity, new Color(.5f, 0, .5f, .1f) }, // Purple
		{ DamageType.Disease, new Color(.5f, .25f, .25f, .1f) }, // Brown
	};

	public override void _Ready()
	{
		CallDeferred(nameof(InitializeCustomSignals));
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Managers.ActionManager.SetSelectedSpell(ActionData);
		}
	}

	public void Setup(ActionData actionData)
	{
		if (actionData == null)
		{
			GD.PrintErr("actionData is null");
			return;
		}

		ActionData = actionData;

		SpellName.Text = actionData.Name;
		SpellDescription.Text = actionData.Description;
		SpellIcon.Texture = actionData.Image;
	}

	private void InitializeCustomSignals()
	{
		Managers.ActionManager.SpellSelected += OnSpellSelected;
	}

	private void OnSpellSelected(string spellName)
	{
		IsSelected = ActionData.Name == spellName;
		UpdateStyle();
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