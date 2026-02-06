using Godot;
using NodeTunnel;
using System.Threading.Tasks;

[NotReplicated]
[GlobalClass, Icon("uid://rqxgol7tuknt")]
public partial class Client : Singleton<Client>
{
	protected private static Player _localPlayer { get; set; }

	public async Task<string> GetId() => (await Server.WaitUntilPeer()).OnlineId;

	[Export]
	protected private int PeerId
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
	protected private async void _setLocalPlayer(ulong id)
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
	/// More easily run a function directly to the client
	/// <para/>@client
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer, 
			CallLocal = true
		)
	]
	protected private void _invoke(ulong objId, StringName method, Variant args)
	{
		GD.Print(objId, IsInstanceIdValid(objId));

		if (IsInstanceIdValid(objId) && InstanceFromId(objId) is GodotObject obj)
		{
			GD.Print("invoked ", method, " in ", obj);
			obj.Call(method, [.. args.AsGodotArray()]);
		}
	}


	/// <inheritdoc cref="_invoke"/>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true
		)
	]
	public Error Invoke(long id, GodotObject obj, StringName method, params Variant[] args)
	{
		return RpcId(id, 
			MethodName._invoke, 
			obj, method, 
			// turns Variant[] into godot array and then into a variant
			// packing it to be unpacked and used as params in _invoke
			Variant.From<Godot.Collections.Array>([.. args])
		);
	}


	/// <inheritdoc cref="_invoke"/>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true
		)
	]
	public Error Invoke(Player player, GodotObject obj, StringName method, params Variant[] args)
		=> Invoke(player.GetPeerId(), obj, method, args);


	/// <inheritdoc cref="_invoke"/>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true
		)
	]
	public Error Invoke(string playerId, GodotObject obj, StringName method, params Variant[] args)
		=> Invoke(players.GetPlayerById(playerId).GetPeerId(), obj, method, args);


	/// <summary>
	/// More easily run a function directly to all clients
	/// <para/>@client
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true
		)
	]
	public Error InvokeAll(GodotObject obj, StringName method, params Variant[] args)
	{
		return Rpc( 
			MethodName._invoke, 
			obj.GetInstanceId(), 
			method, 
			// turns Variant[] into godot array and then into a variant
			// packing it to be unpacked and used as params in _invoke
			Variant.From<Godot.Collections.Array>([.. args])
		);
	}
} 
