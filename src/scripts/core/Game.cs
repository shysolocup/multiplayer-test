using System.Threading.Tasks;
using Godot;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Singleton<Game>
{
	public static async Task<Node> GetGameNode(NodePath nodePath)
	{
		return await GetGameNode<Node>(nodePath);
	}

	public static async Task<T> GetGameNode<T>(NodePath nodePath) where T : Node
	{
		var game = await Instance();
		return game.GetNode<T>(nodePath);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			// Game.Save();
			GetTree().Quit();
		}
	}
}
