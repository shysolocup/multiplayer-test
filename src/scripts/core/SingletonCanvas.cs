using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class SingletonCanvas<T> : CanvasLayer
    where T : CanvasLayer
{
    public static T Me;

    /// <summary>
    /// Single instance of the singleton.
    /// </summary>
    public static async Task<T> Instance() {
        while (Me is null || !IsInstanceValid(Me))
        {
            await Task.Delay(10);
        }

        return Me;
    }


    public override void _Ready()
    {
        if (Me != null && Me != this)
        {
            QueueFree();
            return;
        }

        Me = (T)(object)this;

        Me.TreeExiting += static () => Me = null;

        if (!Engine.IsEditorHint())
        {
            Owner = GetTree().Root;
        }
    }
}