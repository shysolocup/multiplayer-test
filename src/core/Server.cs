using Core;
using Godot;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class Server : Singleton<Server>
{

	public static Replicator Replicator { get; set; }
	public static ServerScriptSystem Scripts { get; set; }
	
	[Signal] public delegate void PlayerConnectedEventHandler(string hostId);

	private static MultiplayerPeer Peer { get; set; }
	public static MultiplayerPeer GetPeer() => Peer;

	public static string GetId() => NodeTunnelBridge.GetOnlineId(Peer);

	private const int Port = 9998;
	private const string DefaultServerAddress = "relay.nodetunnel.io"; 
	[Export] private int MaxPlayers = 20;


	public static string HostId { get; set; }


	public async Task<MultiplayerPeer> ConnectToServer()
	{
		if (Peer is not null) throw new System.Exception("can't make another peer");
		
		Peer = NodeTunnelBridge.NewPeer();
		Multiplayer.MultiplayerPeer = Peer;

		NodeTunnelBridge.ConnectToRelay(DefaultServerAddress, Port);

		// await to be connected
		await NodeTunnelBridge.RelayConnected(Peer);

		var id = GetId();

		GD.PushWarning("initialized with peer id ", id);

		return Peer;
	}

	public override async void _Ready()
	{
		base._Ready();

		Scripts = await ServerScriptSystem.Instance();
		Replicator = await Replicator.Instance();
	}


	/// <summary>
	/// More easily run a function directly to the server
	/// <para/>@client
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = false,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Run(StringName method, params Variant[] args)
	{
		var server = await Instance();
		return server.RpcId(1, method, args);
	}
}
