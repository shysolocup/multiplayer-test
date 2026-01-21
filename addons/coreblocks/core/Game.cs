using System;
using System.Threading.Tasks;
using Godot;
using NodeTunnel;
using System.Reflection;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Singleton<Game>
{

	public override async void _Ready()
	{
		base._Ready();

		if (Engine.IsEditorHint()) return;

		// load singleton systems
		await Load();

		Systems.Server.Multiplayer.PeerConnected += (long id) => {
			GD.Print(id);
		};

		IsLoaded = true;	
	}


	#region Systems
	/// <summary>
	/// Get a singleton system by generic type
	/// <para/><code>
	/// Game.GetSystem&lt;GuiSystem&gt;()
	/// </code>
	/// </summary>
	public static async Task<T> GetSystem<T>() where T : class, IBaseSingleton<GodotObject>
    {
		var t = typeof(T);

		var properties = typeof(Systems).GetProperties(
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
		);

		foreach (var prop in properties)
		{
			if (prop.PropertyType == t)
			{
				var obj = prop.GetValue(null);

				if (obj is not null)
					return (T)obj;

				break;
			}
		}

		// Try to find a public static method called "Instance"
		// Include FlattenHierarchy so inherited statics are found
		var method = t.GetMethod(
			"Instance",
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
		);

		GD.Print("METHOD: ", method);

		if (method is not null)
		{
			// Invoke returns object, cast to Task<T>
			var task = method.Invoke(null, null);
			var outed = await (Task<T>)task;
			return outed;
		}

        throw new Exception($"no system with the name {t.Name} exists.\nif this is a mistake check Game.Systems and make an issue report pls");
    }

	
	/// <summary>
	/// Get up to 10 systems by generic types returning a tuple of each 
	/// <para/><code>
	/// var (guis, workspace, cameras) = Game.GetSystems&lt;GuiSystem, Workspace, CameraSystem&gt;()
	/// </code>
	/// </summary>
	public static async Task<T1> GetSystems<T1>()
		where T1 : class, IBaseSingleton<GodotObject>

		=> await GetSystem<T1>();


	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2)> GetSystems<T1, T2>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>

		=> ( 
			await GetSystem<T1>(), 
			await GetSystem<T2>() 
		);


	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3)> GetSystems<T1, T2, T3>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>

		=> ( 
			await GetSystem<T1>(), 
			await GetSystem<T2>(),
			await GetSystem<T3>()
		);


	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4)> GetSystems<T1, T2, T3, T4>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>

		=> ( 
			await GetSystem<T1>(), 
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>()
		);


	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5)> GetSystems<T1, T2, T3, T4, T5>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>

		=> ( 
			await GetSystem<T1>(), 
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>()
		);


	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5, T6)> GetSystems<T1, T2, T3, T4, T5, T6>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>
		where T6 : class, IBaseSingleton<GodotObject>

		=> ( 
			await GetSystem<T1>(), 
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>(),
			await GetSystem<T6>()
		);

	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5, T6, T7)> GetSystems<T1, T2, T3, T4, T5, T6, T7>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>
		where T6 : class, IBaseSingleton<GodotObject>
		where T7 : class, IBaseSingleton<GodotObject>
		=> (
			await GetSystem<T1>(),
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>(),
			await GetSystem<T6>(),
			await GetSystem<T7>()
		);

	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> GetSystems<T1, T2, T3, T4, T5, T6, T7, T8>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>
		where T6 : class, IBaseSingleton<GodotObject>
		where T7 : class, IBaseSingleton<GodotObject>
		where T8 : class, IBaseSingleton<GodotObject>
		=> (
			await GetSystem<T1>(),
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>(),
			await GetSystem<T6>(),
			await GetSystem<T7>(),
			await GetSystem<T8>()
		);

	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> GetSystems<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>
		where T6 : class, IBaseSingleton<GodotObject>
		where T7 : class, IBaseSingleton<GodotObject>
		where T8 : class, IBaseSingleton<GodotObject>
		where T9 : class, IBaseSingleton<GodotObject>
		=> (
			await GetSystem<T1>(),
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>(),
			await GetSystem<T6>(),
			await GetSystem<T7>(),
			await GetSystem<T8>(),
			await GetSystem<T9>()
		);

	/// <inheritdoc cref="GetSystems{T1}"/>
	public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> GetSystems<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
		where T1 : class, IBaseSingleton<GodotObject>
		where T2 : class, IBaseSingleton<GodotObject>
		where T3 : class, IBaseSingleton<GodotObject>
		where T4 : class, IBaseSingleton<GodotObject>
		where T5 : class, IBaseSingleton<GodotObject>
		where T6 : class, IBaseSingleton<GodotObject>
		where T7 : class, IBaseSingleton<GodotObject>
		where T8 : class, IBaseSingleton<GodotObject>
		where T9 : class, IBaseSingleton<GodotObject>
		where T10 : class, IBaseSingleton<GodotObject>
		=> (
			await GetSystem<T1>(),
			await GetSystem<T2>(),
			await GetSystem<T3>(),
			await GetSystem<T4>(),
			await GetSystem<T5>(),
			await GetSystem<T6>(),
			await GetSystem<T7>(),
			await GetSystem<T8>(),
			await GetSystem<T9>(),
			await GetSystem<T10>()
		);



	/// <summary>
	/// Static namespace of systems
	/// <para/> This is where <see cref="GetSystem"/> and <see cref="GetSystems"/> calls from though GetSystem is awaited so I recommend using that instead of just pulling from here
	/// </summary>
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

	#endregion

	#region Multiplayer

	[Signal] public delegate void StartedHostingEventHandler(string id);
	[Signal] public delegate void JoiningEventHandler(string id);
	[Signal] public delegate void LeftConnectionEventHandler();


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool IsConnected()
		=> Systems.Server is not null 
			&& Server.GetPeer() is NodeTunnelPeer peer
			&& Client.LocalPlayer is not null
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
	private void _leave()
	{
		EmitSignalLeftConnection();
	}


	public async Task Host()
	{
		var peer = await Systems.Server.ConnectToServer();

		peer.Host();
		await peer.WaitUntilHosting();

		var peerId = peer.UniqueId;
		var stringId = peer.OnlineId;

		Server.HostId = stringId;

		guh(peerId, stringId);
		
		EmitSignalStartedHosting(stringId);
	}

	private void guh(long peerId, string stringId)
	{
		Player player = (Player)Replicator.PlayerSpawner.Spawn(new Godot.Collections.Array { 
			peerId, stringId 
		});
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void EmitJoin(long peerId, string stringId)
	{
		if (IsServer())
		{
			guh(peerId, stringId);
		}

		EmitSignalJoining(stringId);
	}

	
	public async Task Join(string hostId)
	{
		var peer = await Systems.Server.ConnectToServer();
		
		peer.Join(hostId);
		await peer.WaitUntilJoined();

		var peerId = peer.UniqueId;
		var stringId = peer.OnlineId;

		Server.HostId = hostId;

		Rpc(MethodName.EmitJoin, peerId, stringId);
	}


	public void Leave()
	{
		var peer = Server.GetPeer();

		if (peer is not null)
		{
			peer.LeaveRoom();

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

	#endregion

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
		=> await GetGameNode<Node>(nodePath);

	public static async Task<T> GetGameNode<T>(NodePath nodePath) where T : Node
	{
		var game = await Instance();
		return game.GetNode<T>(nodePath);
	}

	public void Quit() => GetTree().Quit();


	#region Loading

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


	#endregion


	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			// Game.Save();
			GetTree().Quit();
		}
	}
}
