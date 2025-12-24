using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class Singleton<T> : Node
    where T : Node
{
    private static T _instance;

    /// <summary>
    /// Single instance of the singleton.
    /// </summary>
    public static async Task<T> Instance() {
        while (_instance is null || !IsInstanceValid(_instance))
        {
            await Task.Delay(10);
        }

        return _instance;
    }


    public override void _Ready()
    {
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }

        _instance = (T)(object)this;

        _instance.TreeExiting += static () => _instance = null;

        if (!Engine.IsEditorHint())
        {
            Owner = GetTree().Root;
        }
    }
}