using Godot;
using NodeTunnel;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class Server : Singleton<Server>
{

	public static Replicator Replicator { get; set; }
	public static ServerScriptSystem Scripts { get; set; }
	
	[Signal] public delegate void PlayerConnectedEventHandler(string hostId);

	private static NodeTunnelPeer Peer { get; set; }
	public static NodeTunnelPeer GetPeer() => Peer;

	public static string GetId() => Peer.OnlineId;

	private const int Port = 9998;
	private const string DefaultServerAddress = "relay.nodetunnel.io"; 
	[Export] private int MaxPlayers = 20;


	public static async Task<NodeTunnelPeer> WaitUntilPeer() {
		while (Peer is null || !IsInstanceValid(Peer.Peer))
		{
			await Task.Delay(10);
		}

		return Peer;
	}


	public static string HostId { get; set; }


	public async Task<NodeTunnelPeer> ConnectToServer()
	{
		/// if (Peer is not null) throw new System.Exception("can't make another peer");
		
		Peer = new NodeTunnelPeer();
		Multiplayer.MultiplayerPeer = Peer.Peer;

		Peer.ConnectToRelay(DefaultServerAddress, Port);

		await Peer.WaitUntilRelayConnected();

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

	public Error Invoke(StringName method, params Variant[] args)
	{
		return RpcId(1, method, args);
	}
}
