using Godot;

/// <summary>
/// CharacterCreationSubmit is a button that submits the character creation form.
/// It finds the CharacterCreationManager in the parent hierarchy and calls its SubmitForm method.
/// </summary>
public partial class CharacterCreationSubmit : Button
{
    /// <summary>
    /// Called when the button is pressed to submit the character creation form.
    /// </summary>
    public override void _Pressed()
    {
        var manager = FindParent("CharacterCreationManager") as CharacterCreationManager;
        if (manager != null)
        {
            manager.SubmitForm();
        }
        else
        {
            GD.PrintErr("CharacterCreationManager not found in parent hierarchy.");
        }
    }
}
