using Core;
using Godot;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class Server : Singleton<Server>
{

	public static Replicator Replicator { get; set; }
	public static ServerScriptSystem Scripts { get; set; }

	[Signal] public delegate void StartedHostingEventHandler(string hostId);
	[Signal] public delegate void PlayerConnectedEventHandler(string hostId);

	private static MultiplayerPeer Peer { get; set; }
	public static MultiplayerPeer GetPeer() => Peer;

	public static string GetId() => NodeTunnelBridge.GetOnlineId(Peer);

	[Export] private int Port = 9998;
	[Export] private string DefaultServerAddress = "relay.nodetunnel.io"; 
	[Export] private int MaxPlayers = 20;


	public static string HostId { get; set; }


	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private async Task Init()
	{
		if (Peer is not null) throw new System.Exception(@"
		don;t make another fucking peer you peice of shit I knwo you I 
		know where you live I know that shitty 
		fuckin wendys you go to every saturday and sunday you fat fucking chud
		why do you eat at the same restaurant 2 days in a row every week are you trying to be the manager pleaseget 
		a job fattie
		"
		
		);
		
		Peer = NodeTunnelBridge.NewPeer();
		Multiplayer.MultiplayerPeer = Peer;

		NodeTunnelBridge.ConnectToRelay(Peer, DefaultServerAddress, Port);

		// await to be connected
		await NodeTunnelBridge.RelayConnected(Peer);

		var id = GetId();

		GD.PushWarning("initialized with peer id ", id);
	}

	public override async void _Ready()
	{
		base._Ready();

		await Init();

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
