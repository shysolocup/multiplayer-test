using System.Threading.Tasks;
using Godot;


/// <summary>
/// Base class for 3D singletons with a type argument that allows you to define its inheritance
/// </summary>
public abstract partial class Singleton3D<T> : Node3D, IBaseSingleton<T>
    where T : Node3D
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