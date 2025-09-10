using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// CharacterCreationManager is responsible for managing the character creation process.
/// It handles the player's input for character attributes, updates the available points,
/// and submits the character data to the PlayerManager.
/// </summary>
public partial class CharacterCreationManager : Control
{
	[Export]
	public int MaxTotalPoints = 40; // Each stat starts at 10, so account for that.

	[Export]
	public string AvailablePointsTextSingular;

	[Export]
	public string AvailablePointsTextPlural;

	private LineEdit PlayerNameInput { get; set; }

	private Label AvailablePointsNumberLabel { get; set; }

	private Label AvailablePointsTextLabel { get; set; }

	private Button SubmitButton { get; set; }
	
	private int AvailablePoints
	{
		get
		{
			return MaxTotalPoints - UsedPoints;
		}
	}

	private int UsedPoints
	{
		get
		{
			return GetAllSpinboxes().Sum(spinbox => (int)spinbox.Value);
		}
	}

	public override void _Ready()
	{
		// Initialize form fields and points label
		PlayerNameInput = FindChild("PlayerNameInput") as LineEdit;
		AvailablePointsNumberLabel = FindChild("AvailablePointsNumberLabel") as Label;
		AvailablePointsTextLabel = FindChild("AvailablePointsTextLabel") as Label;
		SubmitButton = FindChild("SubmitButton") as Button;

		Managers.TransitionManager.ToCharacterCreation();
		UpdateAvailablePointsLabels();
		UpdateSubmitButtonState();

		// Connect the player name input text changed signal to update the submit button state.
		PlayerNameInput.TextChanged += (text) => UpdateSubmitButtonState();
	}

	/// <summary>
	/// Submits the character creation form by setting the fields in the CharacterData of the player,
	/// closing the window, and starting the game.
	/// </summary>
	public void SubmitForm()
	{
		Managers.SoundEffectManager.PlayButtonClick();
		Managers.PlayerManager.SetPlayerData(GetPlayerData());
		Managers.TransitionManager.CharacterCreationToGame();
	}

	/// <summary>
	/// Called when a SpinBox value changes to update the available points and labels.
	/// </summary>
	public void OnSpinboxValueChanged()
	{
		UpdateAvailablePointsLabels();
		UpdateSubmitButtonState();
		UpdatePersonalityPointsColour();
	}

	/// <summary>
	/// Updates the labels showing the available points and their text based on the current state of the spinboxes.
	/// </summary>
	private void UpdateAvailablePointsLabels()
	{
		AvailablePointsNumberLabel.Text = AvailablePoints.ToString();
		AvailablePointsTextLabel.Text = AvailablePoints == 1 ? AvailablePointsTextSingular : AvailablePointsTextPlural;
	}

	/// <summary>
	/// Updates the state of the submit button based on the available points and player name input.
	/// </summary>
	private void UpdateSubmitButtonState()
	{
		// Enable or disable the submit button based on the available points and player name input
		SubmitButton.Disabled = AvailablePoints < 0 || PlayerNameInput.Text.Trim().Length == 0;
	}

	/// <summary>
	/// Updates the colour of the personality points based on the current values.
	/// </summary>
	private void UpdatePersonalityPointsColour()
	{
		AvailablePointsNumberLabel.LabelSettings.OutlineColor = AvailablePoints < 0 ? Colors.Red : new Color(0.682f, 0.0f, 1.0f);
	}

	/// <summary>
	/// Retrieves all SpinBox nodes in the character creation manager.
	/// </summary>
	private SpinBox[] GetAllSpinboxes()
	{
		List<SpinBox> spinboxes = GetDescendantsOfType<SpinBox>(this);

		return spinboxes.ToArray();
	}

	/// <summary>
	/// Retrieves the player data from the filled form fields on submission.
	/// </summary>
	private CharacterData GetPlayerData()
	{
		// Retrieve the player data from filled form fields on submission.
		if (PlayerNameInput == null || AvailablePointsNumberLabel == null || AvailablePointsTextLabel == null)
		{
			GD.PrintErr("CharacterCreationManager is not properly initialized.");
			return null;
		}

		var traitValues = GetAllSpinboxes()
			.Where(spinbox => spinbox is PersonalityPointsSpinBox)
			.Cast<PersonalityPointsSpinBox>()
			.ToDictionary(spinbox => spinbox.Trait.Name, spinbox => (int)spinbox.Value)
			.ToList();

		return new CharacterData
		{
			Name = PlayerNameInput.Text,
			BaseBenevolent = traitValues.FirstOrDefault(t => t.Key == "Benevolent").Value,
			BaseCurious = traitValues.FirstOrDefault(t => t.Key == "Curious").Value,
			BaseCharming = traitValues.FirstOrDefault(t => t.Key == "Charming").Value,
			BaseDominant = traitValues.FirstOrDefault(t => t.Key == "Dominant").Value,
			BaseFearless = traitValues.FirstOrDefault(t => t.Key == "Fearless").Value,
			BaseFocused = traitValues.FirstOrDefault(t => t.Key == "Focused").Value,
			BaseGenuine = traitValues.FirstOrDefault(t => t.Key == "Genuine").Value,
			BaseOptimistic = traitValues.FirstOrDefault(t => t.Key == "Optimistic").Value,
		};
	}

	/// <summary>
	/// Recursively finds all descendants of a specific type in the node tree.
	/// </summary>
	private List<T> GetDescendantsOfType<T>(Node root) where T : Node
	{
		List<T> result = new List<T>();
		foreach (Node child in root.GetChildren())
		{
			if (child is T match)
				result.Add(match);

			result.AddRange(GetDescendantsOfType<T>(child));
		}
		return result;
	}
}
