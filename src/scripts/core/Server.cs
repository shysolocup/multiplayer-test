using Core;
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class Server : Singleton<Server>
{

	public static Replicator Replicator { get; set; }
	public static ServerScriptSystem Scripts { get; set; }

	[Signal] public delegate void PlayerConnectedEventHandler(int peerId, Player player);
	[Signal] public delegate void PlayerDisconnectedEventHandler(int peerId, string response);
	[Signal] public delegate void ServerDisconnectedEventHandler(string response);

	[Export] private int Port = 9998;
	[Export] private string DefaultServerAddress = "relay.nodetunnel.io"; 
	[Export] private int MaxPlayers = 20;

	public MultiplayerPeer Peer { get; set; }

	public string HostId { get; set;}

	private async Task Init()
	{
		Peer = NodeTunnelBridge.NewPeer();
		Multiplayer.MultiplayerPeer = Peer;

		NodeTunnelBridge.ConnectToRelay(Peer, DefaultServerAddress, Port);

		await NodeTunnelBridge.RelayConnected(Peer);

		var id = NodeTunnelBridge.GetOnlineId(Peer);

		GD.PushWarning("initialized with peer id ", id);
	}

	public async Task Host()
	{
		NodeTunnelBridge.Host(Peer);

		await NodeTunnelBridge.Hosting(Peer);

		HostId = NodeTunnelBridge.GetOnlineId(Peer);
		DisplayServer.ClipboardSet(HostId.ToString());

		GD.PushWarning($"started hosting at id {HostId}");	
	}

	public async Task Join(string hostId)
	{
		GD.PushWarning($"trying to join {hostId}");	
		NodeTunnelBridge.Join(Peer, hostId);

		await NodeTunnelBridge.Joined(Peer);

		var id = NodeTunnelBridge.GetOnlineId(Peer);

		GD.PushWarning($"peer {id} has joined with the function");	
	}

	public override async void _Ready()
	{
		base._Ready();

		Multiplayer.PeerConnected += (id) =>
		{
			GD.PushWarning($"peer {id} has joined the game from the event side");	
		};

		Multiplayer.PeerDisconnected += (id) =>
		{
			GD.PushWarning($"peer {id} disconnected");		
		};

		Multiplayer.ConnectedToServer += () =>
		{
			GD.PushWarning($"connected to server");	
		};

		Multiplayer.ConnectionFailed += () =>
		{
			GD.PushWarning($"failed to connect to server");	
		};

		Multiplayer.ServerDisconnected += () =>
		{
			GD.PushWarning($"server disconnected");	
		};

		await Init();
		await Host();

		Scripts = await ServerScriptSystem.Instance();
		Replicator = await Replicator.Instance();
	}
}
