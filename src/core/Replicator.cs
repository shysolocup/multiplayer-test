using System.Threading.Tasks;
using Core;
using Godot;

[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{

	[Signal] public delegate void StartedHostingEventHandler(string id);
	[Signal] public delegate void NewConnectionEventHandler(string id);
	[Signal] public delegate void JoiningEventHandler(string id);
	[Signal] public delegate void LeftConnectionEventHandler(string id);

	private static Server server { get; set; }
	private static Client client { get; set; }

	public override async void _Ready()
	{
		base._Ready();

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
	public static bool IsServer(object _ = null)
		=> server is not null && server.Multiplayer.IsServer();


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	public bool IsServer() 
		=> server is not null && server.Multiplayer.IsServer();


	/// <summary>
	/// Checks if the script is running on the client (STATIC)
	/// </summary>
	public static bool IsClient(object _ = null)
		=> !IsServer();

	/// <summary>
	/// Checks if the script is running on the client
	/// </summary>
	public bool IsClient() 
		=> !IsServer();

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public static bool isEditor() 
		=> Engine.IsEditorHint();


	public static bool isTool()
	{
		#if TOOLS
			return true;
		#else
			return false;
		#endif
	}

	public static bool isDebug()
	{
		#if DEBUG
			return true;
		#else
			return false;
		#endif
	}
}
