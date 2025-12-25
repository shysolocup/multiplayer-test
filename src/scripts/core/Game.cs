using System.Threading.Tasks;
using Godot;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Singleton<Game>
{

	public static Workspace Workspace { get; set; }
	public static Players Players { get; set; }
	public static Server Server { get; set; }
	public static Client Client { get; set; }
	public static GlobalStorage GlobalStorage { get; set; }
	public static Replicator Replicator { get; set; }

	public static async Task<Node> GetGameNode(NodePath nodePath)
	{
		return await GetGameNode<Node>(nodePath);
	}

	public static async Task<T> GetGameNode<T>(NodePath nodePath) where T : Node
	{
		var game = await Instance();
		return game.GetNode<T>(nodePath);
	}

	public static bool IsLoaded = false;

	/// <summary>
	/// loads singleton systems
	/// </summary>
	public async void Load()
	{
		// children
		Workspace = await Workspace.Instance();
		Server = await Server.Instance();
		Client = await Client.Instance();
		Players = await Players.Instance();
		GlobalStorage = await GlobalStorage.Instance();
		Replicator = await Replicator.Instance();

		// descendants
		await Players.Instance();
		await ServerScriptSystem.Instance();
		await ClientScriptSystem.Instance();
		await DiscordSystem.Instance();
		await GuiSystem.Instance();
		await ShaderSystem.Instance();
		await Characters.Instance();
		await CameraSystem.Instance();
		await MapSystem.Instance();
		await LightingSystem.Instance();
		await AudioSystem.Instance();
		await Mouse.Instance();
	}

    public override async void _Ready()
    {
        base._Ready();

		// load singleton systems
		Load();

		// run to tell the server that the player loaded
		await Server.Run(Server.MethodName.PlayerLoaded);

		IsLoaded = true;
    }

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			// Game.Save();
			GetTree().Quit();
		}
	}

    public override void _ExitTree()
    {
		// dotnet security
        Workspace = null;
		Players = null;
		Server = null;
		Client = null;
		GlobalStorage = null;
		Replicator = null;
    }

}
