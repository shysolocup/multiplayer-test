using System.Threading.Tasks;
using Godot;
using NodeTunnel;

[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Singleton<Game>
{
	public static class Systems
	{
		public static Workspace Workspace { get; set; }
		public static Players Players { get; set; }
		public static GlobalStorage GlobalStorage { get; set; }
		public static Server Server { get; set; }
		public static Client Client { get; set; }
		public static Replicator Replicator { get; set; }
		public static AudioSystem AudioSystem { get; set; }
		public static MapSystem MapSystem { get; set; }
		public static CameraSystem CameraSystem { get; set; }
		public static Characters Characters { get; set; }
		public static ClientScriptSystem ClientScriptSystem { get; set; }
		public static ServerScriptSystem ServerScriptSystem { get; set; }
		public static GuiSystem GuiSystem { get; set; }
		public static LightingSystem LightingSystem { get; set; }
		public static Mouse Mouse { get; set; }
		public static Screen Screen { get; set; }
		public static ShaderSystem ShaderSystem { get; set; }
		public static FileLib FileLib { get; set; }
		public static JsonLib JsonLib { get; set; }
		public static TaskLib TaskLib { get; set; }
	}

	[Signal] public delegate void StartedHostingEventHandler(string id);
	[Signal] public delegate void NewConnectionEventHandler(string id);
	[Signal] public delegate void JoiningEventHandler(string id);
	[Signal] public delegate void LeftConnectionEventHandler();


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool IsConnected()
		=> Systems.Server is not null 
			&& Server.GetPeer() is NodeTunnelPeer peer
			&& (
				peer.ConnectionState == ConnectionState.Hosting || 
				peer.ConnectionState == ConnectionState.Joined
			);

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool IsConnected(object _ = null)
		=> Game.IsConnected();


	/// <summary>
	/// waits until the player is connected
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static async Task WaitUntilConnected() {
		while (!IsConnected())
		{
			await Task.Delay(10);
		}
	}


	/// <summary>
	/// waits until the player is connected
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public async Task WaitUntilConnected(object _ = null) {
		while (!IsConnected())
		{
			await Task.Delay(10);
		}
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
	private void _leave()
	{
		var remote = Multiplayer.GetRemoteSenderId();
		EmitSignalLeftConnection();
	}


	public async Task Host()
	{
		var peer = await Systems.Server.ConnectToServer();

		peer.Host();

		// await to be connected
		await peer.WaitUntilHosting();

		var id = peer.OnlineId;

		Server.HostId = id;

		EmitSignalStartedHosting(id);
		_join(id, id);
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
		var peer = await Systems.Server.ConnectToServer();

		GD.PushWarning($"trying to join {hostId}");	
		
		peer.Join(hostId);

		await peer.WaitUntilJoined();

		var id = peer.OnlineId;

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

		if (peer is not null)
		{
			peer.LeaveRoom();

			RpcId(1, MethodName._leave);

			GD.PushWarning($"leaving game");		
		}
	}

	/// <summary>
	/// Checks if the script is running on the server (STATIC)
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool IsServer()
		=> 
			Systems.Server is not null &&
			Server.GetPeer() is NodeTunnelPeer peer &&
			peer.ConnectionState == ConnectionState.Hosting;


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool IsServer(object _ = null) 
		=> Game.IsServer();


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
		var loadString = "started loading,";

		// children
		Systems.Workspace = await Workspace.Instance();
		loadString += "loaded workspace,";

		Systems.Server = await Server.Instance();
		loadString += "loaded server,";

		Systems.Client = await Client.Instance();
		loadString += "loaded client,";

		Systems.Players = await Players.Instance();
		loadString += "loaded players,";

		Systems.GlobalStorage = await GlobalStorage.Instance();
		loadString += "loaded global storage,";

		Systems.Replicator = await Replicator.Instance();
		loadString += "loaded replicator,";
		
		Systems.ServerScriptSystem = await ServerScriptSystem.Instance();
		loadString += "loaded server scripts,";

		Systems.ClientScriptSystem = await ClientScriptSystem.Instance();
		loadString += "loaded client scripts,";

		Systems.GuiSystem = await GuiSystem.Instance();
		loadString += "loaded guis,";

		Systems.ShaderSystem = await ShaderSystem.Instance();
		loadString += "loaded shaders,";

		Systems.Characters = await Characters.Instance();
		loadString += "loaded characters,";
		
		Systems.CameraSystem = await CameraSystem.Instance();
		loadString += "loaded cameras,";

		Systems.MapSystem = await MapSystem.Instance();
		loadString += "loaded maps,";

		Systems.LightingSystem = await LightingSystem.Instance();
		loadString += "loaded lightings,";

		Systems.AudioSystem = await AudioSystem.Instance();
		loadString += "loaded audios,";

		Systems.Mouse = await Mouse.Instance();
		loadString += "loaded mouse,";

		Systems.FileLib = await FileLib.Instance();
		loadString += "loaded file lib,";
		
		Systems.TaskLib = await TaskLib.Instance();
		loadString += "loaded task lib,";

		Systems.Screen = await Screen.Instance();
		loadString += "loaded screen,";

		loadString += "game loaded";

		GD.Print(loadString);

		IsLoaded = true;
	}

	public override async void _Ready()
	{
		base._Ready();

		GD.Print("guh");

		NewConnection += id =>
		{
			GD.Print($"received new player joining {id}");	
		};

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
