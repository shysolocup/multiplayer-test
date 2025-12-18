using Godot;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Node
{

	public static Game Instance { get; private set; }
	
	public static Node GetGameNode(NodePath nodePath)
	{
		return Instance.GetNode(nodePath);
	}

	public static T GetGameNode<T>(NodePath nodePath) where T : Node
	{
		return Instance.GetNode<T>(nodePath);
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
