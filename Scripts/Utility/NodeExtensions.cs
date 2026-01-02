using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class NodeExtensions
{
    /// <summary>
    /// Creates an asynchronous delay using Godot's main loop.
    /// Does not block the engine.
    /// </summary>
    /// <param name="node">The Node used to create the timer.</param>
    /// <param name="milliseconds">The delay duration in milliseconds.</param>
    public static async Task Delay(this Node node, int milliseconds)
    {
        // Convert milliseconds to seconds for Godot timer
        float seconds = milliseconds / 1000f;

        // Create a one-shot timer and wait for it
        await node.ToSignal(node.GetTree().CreateTimer(seconds), "timeout");
    }

    /// <summary>
	/// Recursively finds all descendants of a specific type in the node tree.
	/// </summary>
	public static List<T> GetDescendantsOfType<T>(this Node node, Node root) where T : Node
	{
		List<T> result = new List<T>();
		foreach (Node child in root.GetChildren())
		{
			if (child is T match)
				result.Add(match);

			result.AddRange(GetDescendantsOfType<T>(node, child));
		}
		return result;
	}
}