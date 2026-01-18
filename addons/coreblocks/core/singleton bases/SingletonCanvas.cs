using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for canvas layer singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class SingletonCanvas<T> : CanvasLayer, IBaseSingleton<T>
    where T : CanvasLayer
{
    /// <inheritdoc cref="Singleton{T}.Me"/>
    public static T Me;

    
    /// <inheritdoc cref="Singleton{T}.Instance"/>
    public static async Task<T> Instance() {
        while (Me is null || !IsInstanceValid(Me))
            await Task.Delay(10);

        return Me;
    }


    public override void _Ready()
    {
        base._Ready();

        if (Me != null && Me != this)
            return;

        Me = (T)(object)this;
    }
}