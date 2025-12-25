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

	private Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> LocalPlayerDatas = [];

	private Godot.Collections.Dictionary<string, string> BasePlayerInfo = new()
	{
		{ "Name", "PlayerName" },
	};

	private int PlayersLoaded = 0;

	[Export] private int Port = 7000;
	[Export] private string DefaultServerIP = "127.0.0.1"; // IPv4 localhost
	[Export] private int MaxPlayers = 20;

	public ENetMultiplayerPeer ServerHost;


	public override async void _Ready()
	{
		Multiplayer.PeerConnected += OnPlayerConnected;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;
		Multiplayer.ConnectedToServer += OnConnectOk;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
		Multiplayer.ConnectionFailed += OnConnectionFail;

		CreateGame();

		Scripts = await ServerScriptSystem.Instance();
		Replicator = await Replicator.Instance();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		// dotnet security
		Multiplayer.PeerConnected -= OnPlayerConnected;
		Multiplayer.PeerDisconnected -= OnPlayerDisconnected;
		Multiplayer.ConnectedToServer -= OnConnectOk;
		Multiplayer.ServerDisconnected -= OnServerDisconnected;
		Multiplayer.ConnectionFailed -= OnConnectionFail;
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

	public Error CreateGame()
	{
		// server peer
		ServerHost = new ENetMultiplayerPeer();
		Error error = ServerHost.CreateServer(Port, MaxPlayers);

		if (error != Error.Ok)
		{
			return error;
		}

		Multiplayer.MultiplayerPeer = ServerHost;
		LocalPlayerDatas[1] = BasePlayerInfo;
		
		EmitSignal(SignalName.PlayerConnected, 1, BasePlayerInfo);
		
		return Error.Ok;
	}

	private void RemoveMultiplayerPeer()
	{
		Multiplayer.MultiplayerPeer = null;
		GD.PushWarning("cleared players");
		LocalPlayerDatas.Clear();
	}

	// Every peer will call this when they have loaded the game scene.
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void PlayerLoaded()
	{
		if (Multiplayer.IsServer())
		{
			PlayersLoaded += 1;
			
			if (PlayersLoaded == LocalPlayerDatas.Count)
			{
				PlayersLoaded = 0;
			}
		}
	}

	/// <summary>
	/// More easily run a function directly to the server
	/// <para/>@client
	/// </summary>
	public static async Task<Error> Run(StringName method, params Variant[] args)
	{
		var server = await Instance();
		return server.RpcId(1, method, args);
	} 

	// When a peer connects, send them my player info.
	// This allows transfer of all desired data for each player, not only the unique ID.
	private void OnPlayerConnected(long id)
	{
		GD.PushWarning("player connected");
		RpcId(id, MethodName.RegisterPlayer, BasePlayerInfo);
	}

	// client side, initializes data
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RegisterPlayer(Godot.Collections.Dictionary<string, string> newPlayerInfo)
	{
		int id = Multiplayer.GetRemoteSenderId();
		LocalPlayerDatas[id] = newPlayerInfo;
		EmitSignal(SignalName.PlayerConnected, id);
	}

	private void OnPlayerDisconnected(long id)
	{
		LocalPlayerDatas.Remove(id);
		EmitSignal(SignalName.PlayerDisconnected, id);
	}

	private void OnConnectOk()
	{
		int peerId = Multiplayer.GetUniqueId();
		LocalPlayerDatas[peerId] = BasePlayerInfo;
		EmitSignal(SignalName.PlayerConnected, peerId, BasePlayerInfo);
	}

	private void OnConnectionFail()
	{
		Multiplayer.MultiplayerPeer = null;
	}

	private void OnServerDisconnected()
	{
		Multiplayer.MultiplayerPeer = null;
		LocalPlayerDatas.Clear();
		EmitSignal(SignalName.ServerDisconnected);
	}
}
