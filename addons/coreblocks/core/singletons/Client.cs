using Godot;
using NodeTunnel;
using System.Threading.Tasks;


[GlobalClass, Icon("uid://rqxgol7tuknt")]
public partial class Client : Singleton<Client>
{
	private static Player _localPlayer { get; set; }

	public async Task<string> GetId() => (await Server.WaitUntilPeer()).OnlineId;

	[Export]
	private int PeerId
	{
		get => Server.GetPeer() is NodeTunnelPeer peer ? peer.UniqueId : -1;
		set {}
	}

	public static GuiSystem Gui { get; set; }
	public static CameraSystem Cameras { get; set; }
	public static Camera3D CurrentCamera { get => Cameras.CurrentCamera; }
	public static ClientScriptSystem Scripts { get; set; }
	public static Replicator Replicator { get; set; }

	private Players players { get; set; }

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

		if (Engine.IsEditorHint()) return;

		Gui ??= await GuiSystem.Instance();
		Cameras ??= GetNode<CameraSystem>("./cameras");
		Scripts ??= await ClientScriptSystem.Instance();
		Replicator ??= await Replicator.Instance();
		players ??= await Players.Instance();
		

		GD.PushWarning("loaded client stuff");
	}


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true
		)
	]
	private async void _setLocalPlayer(ulong id)
	{
		if (IsInstanceIdValid(id) && InstanceFromId(id) is Player player && player.GetId() == await GetId())
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
			CallLocal = false,
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public Error SetLocalPlayer(Player player, object _ = null)
	{
		return RpcId(player.GetPeerId(), MethodName._setLocalPlayer, player.GetInstanceId());
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

	public Error Invoke(long id, StringName method, params Variant[] args)
	{
		return RpcId(id, method, args);
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

	public Error Invoke(string id, StringName method, params Variant[] args)
	{
		var player = players.GetPlayerById(id);

		return RpcId(player.GetPeerId(), method, args);
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
	
	public Error Invoke(Player player, StringName method, params Variant[] args)
	{
		return Invoke(player.GetPeerId(), method, args);
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

	public Error Invoke<T>(long id, T obj, StringName method, params Variant[] args) where T : Node
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

	public Error Invoke<T>(string id, T obj, StringName method, params Variant[] args)
	{
		var player = players.GetPlayerById(id);

		return RpcId(player.GetPeerId(), method, args);
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
	
	public Error Invoke(Player player, GodotObject obj, StringName method, params Variant[] args)
	{
		return Invoke(player.GetPeerId(), method, args);
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

	public void InvokeAll(StringName method, params Variant[] args)
	{
		Rpc(method, args);
	}
} 
