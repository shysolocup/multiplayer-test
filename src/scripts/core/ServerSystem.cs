using Core;
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class ServerSystem : Singleton<ServerSystem>
{

	[Signal] public delegate void PlayerConnectedEventHandler(int peerId, Player player);
	[Signal] public delegate void PlayerDisconnectedEventHandler(int peerId, string response);
	[Signal] public delegate void ServerDisconnectedEventHandler(string response);


	[Export] private int Port = 7000;
	[Export] private string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	[Export] private int MaxPlayers = 20;


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private async void RegisterPlayer()
	{
		var player = await Players.MakePlayer();
		int id = Multiplayer.GetRemoteSenderId();
		EmitSignal(SignalName.PlayerConnected, id, player);
	}


	private void _ClientConnected(long id)
	{
		RpcId(id, MethodName.RegisterPlayer);
	}

	private async void _ClientDisconnected(long id)
	{
		var player = await Players.GetPlayerById(id);
		EmitSignal(SignalName.PlayerDisconnected, id);
	}

	private void _ServerConnected()
	{
		
	}

	private void _Fail()
	{
		
	}

	private void _ServerDisconnected()
	{
		
	}

	private Error JoinGame(string address = "")
	{
		if (string.IsNullOrEmpty(address))
		{
			address = DefaultServerIP;
		}

		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(address, Port);

		if (error != Error.Ok)
		{
			return error;
		}

		Multiplayer.MultiplayerPeer = peer;
		return Error.Ok;
	}

	private async Task<Error> CreateServer()
	{
		var peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Port, MaxPlayers);

		if (error != Error.Ok)
		{
			return error;
		}

		var player = await Players.MakePlayer();

		Multiplayer.MultiplayerPeer = peer;
		EmitSignal(SignalName.PlayerConnected, 1, player);
		
		return Error.Ok;
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private async void EndServer()
	{
		Multiplayer.MultiplayerPeer = null;
		var players = await Players.Instance();
		
		EmitSignal(SignalName.PlayerDisconnected, Multiplayer.GetUniqueId());

		players.ClearChildren();
	}
 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += _ClientConnected;
		Multiplayer.PeerDisconnected += _ClientDisconnected;
		Multiplayer.ConnectedToServer += _ServerConnected;
		Multiplayer.ServerDisconnected += _ServerDisconnected;
		Multiplayer.ConnectionFailed += _Fail;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
