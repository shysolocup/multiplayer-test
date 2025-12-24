using System.Threading.Tasks;
using Godot;

[GlobalClass, Icon("uid://dda83k1ok1hyq")]
public partial class GlobalStorage : Singleton<GlobalStorage>
{
	public override void _Ready()
	{
		base._Ready();
	}

	/// <summary>
	/// Gets relative remotes by directory eg: GlobalSystem.GetRemote("Dance")
	/// </summary>
	/// <param name="directory"></param>
	/// <returns></returns>
	public static async Task<Remote> GetRemote(string directory)
	{
		var rep = await Instance();
		return rep.GetNode<Remote>($"./{directory}");
	}
}
