using Godot;
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
}