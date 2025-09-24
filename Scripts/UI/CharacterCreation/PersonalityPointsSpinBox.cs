using Godot;

/// <summary>
/// PersonalityPointsSpinBox is a custom SpinBox for managing personality points in character creation.
/// It ensures that the value is within the range of -10 to 10 and notifies
/// the CharacterCreationManager when the value changes.
/// </summary>
public partial class PersonalityPointsSpinBox : SpinBox
{
    /// <summary>
    /// The personality trait associated with this SpinBox.
    /// </summary>
    [Export]
    public PersonalityTrait Trait { get; set; }

    public override void _Ready()
    {
        ValueChanged += OnSpinboxValueChanged;
    }

    /// <summary>
    /// Handles the value change event of the SpinBox.
    /// </summary>
    private void OnSpinboxValueChanged(double value)
    {
        // Ensure the value is within the range of -10 to 10
        if (value < -10 || value > 10)
        {
            GD.PrintErr("Value must be between -10 and 10.");
            return;
        }

        // Notify the CharacterCreationManager about the change
        var manager = FindParent("CharacterCreationManager") as CharacterCreationManager;
        if (manager != null)
        {
            manager.OnSpinboxValueChanged();
            UpdatePersonalityLabel();
        }
    }

    /// <summary>
    /// Updates the label for this personality trait based on the current value.
    /// </summary>
    private void UpdatePersonalityLabel()
    {
        if (Value < 0)
        {
            GetParent().GetNode<Label>("Label").Text = Trait.NegativeName;
        }
        else
        {
            GetParent().GetNode<Label>("Label").Text = Trait.Name;
        }
    }
}
