using Godot;
using Godot.Collections;

/// <summary>
/// Added to the root note of a scene to expose its components to other scripts.
/// </summary>
public partial class ComponentExposer : Node
{
    /// <summary>
    /// A dictionary to hold references to exposed components.
    /// </summary>
    private Dictionary<string, Node> Components = [];

    /// <summary>
    /// Exposes a child node of the scene by its name.
    /// </summary>
    public T GetComponent<T>(string componentName) where T : Node
    {
        // Check if the component is already cached
        if (Components.TryGetValue(componentName, out Node value))
        {
            return value as T;
        }

        // Try to get the component from the scene tree
        if (FindChild(componentName) is T component)
        {
            Components[componentName] = component;
            return component;
        }

        // If not found, log an error
        GD.PrintErr("Component not found: " + componentName);
        return null;
    }
}
