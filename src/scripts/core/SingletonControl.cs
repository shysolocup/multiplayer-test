using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class SingletonControl<T> : Control
    where T : Control
{
    private static T _instance;

    /// <summary>
    /// Single instance of the singleton.
    /// </summary>
    public static Task<T> Instance() => Task.FromResult(_instance);


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