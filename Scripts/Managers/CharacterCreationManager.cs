using System.Collections.Generic;
using System.Linq;
using DungeonRPG.Blessings.Enums;
using DungeonRPG.Characters;
using Godot;

/// <summary>
/// CharacterCreationManager is responsible for managing the character creation process.
/// It handles the player's input for character attributes, updates the available points,
/// and submits the character data to the PlayerManager.
/// </summary>
public partial class CharacterCreationManager : Control
{
	/// <summary>
    /// Skips the character creation process and goes directly to the game.
	/// Used for testing purposes.
    /// </summary>
	[Export]
	public bool SkipCharacterCreation = false;

	/// <summary>
    /// The maximum total points available for character attribute allocation.
    /// </summary>
	[Export]
	public int MaxTotalPoints = 40; // Each stat starts at 10, so account for that.

	/// <summary>
    /// The text to display when there is one available point.
    /// </summary>
	[Export]
	public string AvailablePointsTextSingular;

	/// <summary>
    /// The text to display when there are multiple available points.
    /// </summary>
	[Export]
	public string AvailablePointsTextPlural;

	/// <summary>
	/// The ComponentExposer used to access UI components related to available points.
	/// </summary>
	[Export]
	private ComponentExposer AvailablePointsExposer;

	/// <summary>
    /// The LineEdit for player name input.
    /// </summary>
	private LineEdit PlayerNameInput { get; set; }

	/// <summary>
    /// The Label displaying the number of available points.
    /// </summary>
	private Label AvailablePointsNumberLabel => AvailablePointsExposer.GetComponent<Label>(Components.AvailablePointsNumberLabel);

	/// <summary>
    /// The Label displaying the text for available points.
    /// </summary>
	private Label AvailablePointsTextLabel => AvailablePointsExposer.GetComponent<Label>(Components.AvailablePointsTextLabel);

	/// <summary>
    /// The Warning indicating unbalanced points.
    /// </summary>
	private Control WarningContainer => AvailablePointsExposer.GetComponent<Control>(Components.WarningContainer);

	/// <summary>
    /// The Submit Button for submitting the character creation form.
    /// </summary>
	private Button SubmitButton { get; set; }
	
	/// <summary>
    /// Calculates the number of available points left for allocation.
    /// </summary>
	private int AvailablePoints
	{
		get
		{
			return MaxTotalPoints - UsedPoints;
		}
	}

	/// <summary>
    /// Calculates the number of points already used in the spinboxes.
    /// </summary>
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
		SubmitButton = FindChild("SubmitButton") as Button;

		Managers.TransitionManager.ToCharacterCreation();
		UpdateAvailablePoints();
		UpdateSubmitButtonState();

		// Connect the player name input text changed signal to update the submit button state.
		PlayerNameInput.TextChanged += (text) => UpdateSubmitButtonState();

		if (SkipCharacterCreation)
		{
			CallDeferred(nameof(SubmitForm));
		}
	}

	/// <summary>
	/// Submits the character creation form by setting the fields in the CharacterData of the player,
	/// closing the window, and starting the game.
	/// </summary>
	public void SubmitForm()
	{
        _ = Managers.SoundEffectManager.PlayButtonClick();
		Managers.PlayerManager.SetPlayerData(GetPlayerData());
		var build = GenerateCharacterBuild();
		ApplyBuild(build);
		Managers.TransitionManager.CharacterCreationToGame();
	}

	/// <summary>
	/// Called when a SpinBox value changes to update the available points and labels.
	/// </summary>
	public void OnSpinboxValueChanged()
	{
		UpdateAvailablePoints();
		UpdateSubmitButtonState();
	}

	/// <summary>
	/// Updates the labels showing the available points and their text based on the current state of the spinboxes.
	/// </summary>
	private void UpdateAvailablePoints()
	{
		AvailablePointsNumberLabel.Text = AvailablePoints.ToString();
		AvailablePointsTextLabel.Text = AvailablePoints == 1 ? AvailablePointsTextSingular : AvailablePointsTextPlural;

		WarningContainer.Visible = AvailablePoints != 0;
	}

	/// <summary>
	/// Updates the state of the submit button based on the available points and player name input.
	/// </summary>
	private void UpdateSubmitButtonState()
	{
		// Enable or disable the submit button based on the available points and player name input
		SubmitButton.Disabled = AvailablePoints != 0 || PlayerNameInput.Text.Trim().Length == 0;
	}

	/// <summary>
	/// Retrieves all SpinBox nodes in the character creation manager.
	/// </summary>
	private SpinBox[] GetAllSpinboxes()
	{
		List<SpinBox> spinboxes = this.GetDescendantsOfType<SpinBox>(this);

		return spinboxes.ToArray();
	}

	/// <summary>
	/// Retrieves the player data from the filled form fields.
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

		CharacterData characterData = Managers.PlayerManager.GetPlayer().CharacterData;

		characterData.Name = PlayerNameInput.Text == "" ? "Unnamed Hero" : PlayerNameInput.Text;
		characterData.BaseBenevolent = traitValues.FirstOrDefault(t => t.Key == "Benevolent").Value;
		characterData.BaseCurious = traitValues.FirstOrDefault(t => t.Key == "Curious").Value;
		characterData.BaseCharming = traitValues.FirstOrDefault(t => t.Key == "Charming").Value;
		characterData.BaseDominant = traitValues.FirstOrDefault(t => t.Key == "Dominant").Value;
		characterData.BaseFearless = traitValues.FirstOrDefault(t => t.Key == "Fearless").Value;
		characterData.BaseFocused = traitValues.FirstOrDefault(t => t.Key == "Focused").Value;
		characterData.BaseGenuine = traitValues.FirstOrDefault(t => t.Key == "Genuine").Value;
		characterData.BaseOptimistic = traitValues.FirstOrDefault(t => t.Key == "Optimistic").Value;
		
		return characterData;
	}

	/// <summary>
	/// Sets the starting blessings and spells on the player based on the provided build.
	/// </summary>
	private static void ApplyBuild(Build build)
	{
		foreach (var blessing in build.Blessings)
		{
			Managers.ManaSourceManager.AddBlessing(blessing);
		}

		foreach (var spell in build.Spells)
		{
			Managers.SpellBookManager.AddSpell(spell.Data);
		}
	}

	/// <summary>
	/// Generates the character build based on the selected personality traits.
	/// </summary>
	private Build GenerateCharacterBuild()
	{
		// Builds will be generated based on the personality traits of the player
		// See https://www.notion.so/Damage-types-and-domains-285b94d5324680a5b55dd0b70998646c for the personality traits related to the different Godly domains
		// We assign points to the domain based on each character trait
		// If one of the domains has a much higher value than the others, you'll get a 1-domain build
		// If it's more equal, 2-domain build.
		// There is no 3 or 4 domain starting build

		var zerPoints = 0;
		var haminPoints = 0;
		var jaddisPoints = 0;
		var calinaPoints = 0;

		var traitSpinBoxes = GetAllSpinboxes()
			.Where(spinbox => spinbox is PersonalityPointsSpinBox)
			.Cast<PersonalityPointsSpinBox>()
			.ToDictionary(spinbox => spinbox.Trait.Name, spinbox => (int)spinbox.Value)
			.ToList();

		// Match chosen personality traits to their respective domains and tally points
		foreach (var trait in traitSpinBoxes)
		{
			switch (trait.Key)
			{
				case "Genuine":
				case "Optimistic":
					zerPoints += trait.Value;
					break;
				case "Curious":
				case "Focused":
					haminPoints += trait.Value;
					break;
				case "Charming":
				case "Benevolent":
					jaddisPoints += trait.Value;
					break;
				case "Dominant":
				case "Fearless":
					calinaPoints += trait.Value;
					break;
			}
		}

		// Get the total points to calculate proportions
		var totalPoints = zerPoints + haminPoints + jaddisPoints + calinaPoints;
		if (totalPoints != 0)
		{
			// This should never be non-zero, but just in case to avoid division by zero
			GD.PrintErr("Total trait points is non-zero, cannot generate character build.");
			return null;
		}

		// Determine the dominant domains based on the points
		var domainPoints = new Dictionary<Domain, int>
		{
			{ Domain.Zer, zerPoints },
			{ Domain.Hamin, haminPoints },
			{ Domain.Jaddis, jaddisPoints },
			{ Domain.Calina, calinaPoints }
		};

		var sortedDomains = domainPoints.OrderByDescending(dp => dp.Value).ToList();
		var topDomain = sortedDomains[0];
		var secondDomain = sortedDomains[1];

		// Assign starting build based on dominant domains
		if (topDomain.Value >= secondDomain.Value * 1.5)
		{
			// 1-domain build
			return Builds.GetSingleDomainBuild(topDomain.Key);
		}
		else
		{
			// 2-domain build
			return Builds.GetDualDomainBuild(topDomain.Key, secondDomain.Key);
		}
	}
}
