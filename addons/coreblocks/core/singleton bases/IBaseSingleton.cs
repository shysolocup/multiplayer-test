using System.Threading.Tasks;

public interface IBaseSingleton<out T>
{
    /// <summary>
    /// Instance of the singleton
    /// </summary>
    public static T Me;
}