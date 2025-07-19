using Godot;

/// <summary>
/// PersonalityTrait is a resource that defines a personality trait for a character.
/// It includes the trait's name and a negative version of the name for when the trait is
/// below 0.
/// </summary>
[GlobalClass]
public partial class PersonalityTrait : Resource
{
    [Export]
    public string Name { get; set; } = "Default Trait";

    [Export]
    public string NegativeName { get; set; } = "Negative Trait";
}
