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
	
	[Signal] public delegate void StartedHostingEventHandler(string id);
	[Signal] public delegate void NewConnectionEventHandler(string id);
	[Signal] public delegate void JoiningEventHandler(string id);
	[Signal] public delegate void LeftConnectionEventHandler(string id);

	private static Server server { get; set; }
	private static Client client { get; set; }

	public override async void _Ready()
	{
		base._Ready();

		if (Engine.IsEditorHint()) return;

		server = await Server.Instance();
		client = await Client.Instance();

		NewConnection += id =>
		{
			GD.Print($"received new player joining {id}");	
		};
	}


	public static bool IsConnected()
		=> server is not null && server.Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;


	/// <summary>
	/// waits until the player is connected
	/// </summary>
	public static async Task WaitUntilConnected() {
		await NodeTunnelBridge.RelayConnected();
	}


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true
		)
	]
	private async void _join(string id, string hostId)
	{
		var remote = Multiplayer.GetRemoteSenderId();

		EmitSignalNewConnection(id);

		if (IsServer())
		{
			GD.Print("trying to make the player");
			var player = await Players.MakePlayer(remote, id);
		}
	}

	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = true
		)
	]
	private void _leave(string id)
	{
		var remote = Multiplayer.GetRemoteSenderId();
		EmitSignalLeftConnection(id);
	}


	public async Task Host()
	{
		var peer = await server.ConnectToServer();

		NodeTunnelBridge.Host(peer);

		// await to be connected
		await NodeTunnelBridge.Hosting(peer);

		var id = NodeTunnelBridge.GetOnlineId(peer);

		Server.HostId = id;
		// DisplayServer.ClipboardSet(HostId.ToString());

		EmitSignalStartedHosting(id);
		
		RpcId(1, MethodName._join, id, id);

		GD.PushWarning($"started hosting at id {id}");	
	}


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = true
		)
	]
	public async Task Join(string hostId)
	{
		var peer = await server.ConnectToServer();

		GD.PushWarning($"trying to join {hostId}");	
		NodeTunnelBridge.Join(peer, hostId);

		await NodeTunnelBridge.Joined(peer);

		var id = Server.GetId();

		EmitSignalJoining(id);

		RpcId(1, MethodName._join, id, hostId);

		GD.PushWarning($"peer {id} has joined with the function");	
	}


	[
		Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)
	]
	public async Task Leave()
	{
		var peer = Server.GetPeer();
		var id = Server.GetId();

		NodeTunnelBridge.Leave(peer);

		RpcId(1, MethodName._leave, id);

		GD.PushWarning($"leaving game");	
	}

	/// <summary>
	/// Checks if the script is running on the server (STATIC)
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool IsServer(object _ = null)
		=> server is not null && server.Multiplayer.IsServer();


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool IsServer() 
		=> server is not null && server.Multiplayer.IsServer();


	/// <summary>
	/// Checks if the script is running on the client (STATIC)
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool IsClient(object _ = null)
		=> !IsServer();

	/// <summary>
	/// Checks if the script is running on the client
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool IsClient() 
		=> !IsServer();

	/// <summary>
	/// if it's running in the editor
	/// </summary>
	/// <returns></returns>
	public static bool IsEditor() 
		=> Engine.IsEditorHint();


	/// <summary>
	/// if it's currently running as a tool using <c>#if TOOLS</c>
	/// </summary>
	/// <returns></returns>
	public static bool IsTool()
	{
		#if TOOLS
			return true;
		#else
			return false;
		#endif
	}


	/// <summary>
	/// if it's currently running in debug using <c>#if DEBUG</c>
	/// </summary>
	/// <returns></returns>
	public static bool IsDebug()
	{
		#if DEBUG
			return true;
		#else
			return false;
		#endif
	}

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
