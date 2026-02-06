using Godot;
using NodeTunnel;
using System;
using System.Linq;
using System.Threading.Tasks;

[NotReplicated]
[GlobalClass, Icon("uid://cx7nfcxc0as46")]
public partial class Server : Singleton<Server>
{

	public static Replicator Replicator { get; set; }
	public static ServerScriptSystem Scripts { get; set; }
	public static Players players { get; set; }
	private static Client client { get; set; }
	
	[Signal] public delegate void PlayerConnectedEventHandler(string hostId);

	protected private static NodeTunnelPeer Peer { get; set; }
	public static NodeTunnelPeer GetPeer() => Peer;

	[Export]
	protected private string HostId { get; set; }

	public void SetHostId(string hostId) => HostId = hostId;


	/// <summary>
	/// Gets the host's string id
	/// </summary>
	public string GetHostId() => HostId;

	/// <summary>
	/// Gets the host's peer id
	/// </summary>
	public long GetHostPeerId() => players.GetPlayerById(HostId).GetPeerId();

	protected private const int Port = 9998;
	protected private const string DefaultServerAddress = "relay.nodetunnel.io"; 
	[Export] private int MaxPlayers = 20;


	public static async Task<NodeTunnelPeer> WaitUntilPeer() {
		while (Peer is null || !IsInstanceValid(Peer.Peer))
		{
			await Task.Delay(10);
		}

		return Peer;
	}


	public async Task<NodeTunnelPeer> ConnectToServer()
	{
		/// if (Peer is not null) throw new System.Exception("can't make another peer");
		
		Peer = new NodeTunnelPeer();
		Multiplayer.MultiplayerPeer = Peer.Peer;

		Peer.ConnectToRelay(DefaultServerAddress, Port);

		await Peer.WaitUntilRelayConnected();

		var id = await client.GetId();

		GD.PushWarning("initialized with peer id ", id);

		return Peer;
	}

	public override async void _Ready()
	{
		base._Ready();

		Scripts = await ServerScriptSystem.Instance();
		Replicator = await Replicator.Instance();
		players = await Players.Instance();
		client = await Client.Instance();
	}


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = true
		)
	]
	protected private void _invoke(ulong objId, StringName method, Variant args)
	{
		GD.Print(objId, IsInstanceIdValid(objId));

		if (Game.IsServer() && IsInstanceIdValid(objId) && InstanceFromId(objId) is GodotObject obj)
		{
			GD.Print("invoked ", method, " in ", obj);
			obj.Call(method, [.. args.AsGodotArray()]);
		}
	}


	/// <summary>
	/// More easily run a function directly to the server
	/// <para/>@client
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = true
		)
	]

	public Error Invoke(GodotObject obj, StringName method, params Variant[] args)
	{
		return RpcId(1, 
			MethodName._invoke, 
			obj.GetInstanceId(), 
			method,
			// turns Variant[] into godot array and then into a variant
			// packing it to be unpacked and used as params in _invoke
			Variant.From<Godot.Collections.Array>([.. args])
		);
	}
}
