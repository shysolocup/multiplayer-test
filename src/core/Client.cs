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
	public static Camera3D Camera { get => Cameras.Camera; }
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
	private void _setLocalPlayer(Player player)
	{
		GD.Print(Multiplayer.GetRemoteSenderId());
		LocalPlayer = player;
	}


	/// <summary>
	/// Sets the local player of the client using the player's built in id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> SetLocalPlayer(Player player)
	{
		var client = await Instance();
		return client.RpcId(player.GetPeerId(), MethodName._setLocalPlayer, player);
	}


	/// <summary>
	/// Sets the local player of the client using a given id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> SetLocalPlayer(string id, Player player)
	{
		var client = await Instance();
		return client.RpcId(long.Parse(id), MethodName._setLocalPlayer, player);
	}


	/// <summary>
	/// Sets the local player of the client using a given id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> SetLocalPlayer(long id, Player player)
	{
		var client = await Instance();
		return client.RpcId(id, MethodName._setLocalPlayer, player);
	}


	/// <summary>
	/// More easily run a function directly to the client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = false,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Run(long id, StringName method, params Variant[] args)
	{
		var client = await Instance();
		return client.RpcId(id, method, args);
	}


	/// <summary>
	/// More easily run a function directly to the client
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = false,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public static async Task<Error> Run(string id, StringName method, params Variant[] args)
	{
		var client = await Instance();
		return client.RpcId(long.Parse(id), method, args);
	}


	/// <summary>
	/// More easily run a function directly to the server
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = false,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public static async Task<Error> Run(Player player, StringName method, params Variant[] args)
	{
		return await Run(player.GetId(), method, args);
	}
}
