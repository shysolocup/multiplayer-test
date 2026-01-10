using System.Threading.Tasks;
using Godot;

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

	public static async Task Loaded() {
        while (!IsLoaded)
        {
            await Task.Delay(10);
        }
    }

	public static bool IsLoaded = false;

	/// <summary>
	/// loads singleton systems
	/// </summary>
	public async Task Load()
	{
		GD.Print("started loading");

		// children
		Workspace = await Workspace.Instance();
		GD.Print("loaded workspace");

		Server = await Server.Instance();
		GD.Print("loaded server");

		Client = await Client.Instance();
		GD.Print("loaded client");

		Players = await Players.Instance();
		GD.Print("loaded players");

		GlobalStorage = await GlobalStorage.Instance();
		GD.Print("loaded global storage");

		Replicator = await Replicator.Instance();
		GD.Print("loaded replicator");
		
		await ServerScriptSystem.Instance();
		GD.Print("loaded server scripts");

		await ClientScriptSystem.Instance();
		GD.Print("loaded client scripts");

		await GuiSystem.Instance();
		GD.Print("loaded guis");

		await ShaderSystem.Instance();
		GD.Print("loaded shaders");

		await Characters.Instance();
		GD.Print("loaded characters");
		
		await CameraSystem.Instance();
		GD.Print("loaded cameras");

		await MapSystem.Instance();
		GD.Print("loaded maps");

		await LightingSystem.Instance();
		GD.Print("loaded lightings");

		await AudioSystem.Instance();
		GD.Print("loaded audios");

		await Mouse.Instance();
		GD.Print("loaded mouse");

		GD.Print("game loaded");

		IsLoaded = true;
	}

	public override async void _Ready()
	{
		base._Ready();

		GD.Print("guh");

		// load singleton systems
		await Load();

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
}
