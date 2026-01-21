using System.Threading.Tasks;


/// <summary>
/// a behavior script mixed with a singleton
/// </summary>
public abstract partial class ModuleBehavior<T> : Behavior
    where T : Behavior
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