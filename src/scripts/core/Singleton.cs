using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class Singleton<T> : Node
    where T : Node
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
        base._Ready();

        if (Me != null && Me != this)
        {
            return;
        }

        Me = (T)(object)this;
    }
}