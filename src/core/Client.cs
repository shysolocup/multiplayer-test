using Core;
using Godot;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://rqxgol7tuknt")]
public partial class Client : Singleton<Client>
{
	private static Player _localPlayer { get; set; }

	public string GetId() => NodeTunnelBridge.GetOnlineId();

	public static GuiSystem Gui { get; set; }
	public static CameraSystem Cameras { get; set; }
	public static Camera3D CurrentCamera { get => Cameras.CurrentCamera; }
	public static ClientScriptSystem Scripts { get; set; }
	public static Replicator Replicator { get; set; }

	public static Player LocalPlayer
	{
		get => _localPlayer;
		private set
		{
			_localPlayer?.QueueFree();
			_localPlayer = value;
		}
	}

	[Export]
	private Player _local_Player
	{
		get => LocalPlayer;
		set
		{
			LocalPlayer = value;
		}
	}


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();

		Gui ??= await GuiSystem.Instance();
		Cameras ??= GetNode<CameraSystem>("./cameras");
		Scripts ??= await ClientScriptSystem.Instance();
		Replicator ??= await Replicator.Instance();

		GD.PushWarning("loaded client stuff");
	}


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true
		)
	]
	private void _setLocalPlayer(ulong id)
	{
		var instance = InstanceFromId(id);

		if (instance is Player player && Multiplayer.GetUniqueId() == Multiplayer.GetRemoteSenderId())
		{
			GD.Print("set local player");
			LocalPlayer = player;	
		}
	}


	/// <summary>
	/// Sets the local player of the client using the player's built in id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> SetLocalPlayer(Player player)
	{
		var client = await Instance();
		return client.RpcId(player.GetPeerId(), MethodName._setLocalPlayer, player.GetInstanceId());
	}


	/// <summary>
	/// Sets the local player of the client using a given id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> SetLocalPlayer(long id, Player player)
	{
		var client = await Instance();
		return client.RpcId(id, MethodName._setLocalPlayer, player);
	}


	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Invoke(long id, StringName method, params Variant[] args)
	{
		var client = await Instance();
		return client.RpcId(id, method, args);
	}


	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Invoke(string id, StringName method, params Variant[] args)
	{
		var client = await Instance();
		var player = await Players.GetPlayerById(id);

		return client.RpcId(player.GetPeerId(), method, args);
	}


	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> Invoke(Player player, StringName method, params Variant[] args)
	{
		return await Invoke(player.GetPeerId(), method, args);
	}


	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Invoke<T>(long id, T obj, StringName method, params Variant[] args) where T : Node
	{
		return obj.RpcId(id, method, args);
	}


	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Invoke<T>(string id, T obj, StringName method, params Variant[] args)
	{
		var client = await Instance();
		var player = await Players.GetPlayerById(id);

		return client.RpcId(player.GetPeerId(), method, args);
	}

	/// <summary>
	/// More easily run a function directly to a client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> Invoke(Player player, GodotObject obj, StringName method, params Variant[] args)
	{
		return await Invoke(player.GetPeerId(), method, args);
	}

	/// <summary>
	/// More easily run a function directly to ALL players
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task InvokeAll(StringName method, params Variant[] args)
	{
		var client = await Instance();
		foreach (var id in client.Multiplayer.GetPeers())
		{
			client.RpcId(id, method, args);
		}
	}
} 
