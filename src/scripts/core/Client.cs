using Core;
using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://rqxgol7tuknt")]
public partial class Client : Singleton<Client>
{
	private static Player _localPlayer;

	public static GuiSystem Gui { get; set; }
	public static Camera3D Camera { get; set; }
	public static ClientScriptSystem Scripts { get; set; }
	public static DiscordSystem DiscordRPC { get; set; }
	public static Replicator Replicator { get; set; }
	public static Mouse Mouse { get; set; }

	public static Player LocalPlayer
	{
		get => _localPlayer;
		private set
		{
			_localPlayer?.QueueFree();
			_localPlayer = value;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		// dotnet security
		Gui = null;
		Camera = null;
		Scripts = null;
		DiscordRPC = null;
		Replicator = null;
		Mouse = null;
	}


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();

		Gui ??= await GuiSystem.Instance();
		Camera ??= GetNode<Camera3D>("./camera");
		Scripts ??= await ClientScriptSystem.Instance();
		DiscordRPC ??= await DiscordSystem.Instance();
		Replicator ??= await Replicator.Instance();
		Mouse ??= await Mouse.Instance();

		GD.PushWarning("loaded client stuff");
	}


	/// <summary>
	/// More easily run a function directly to the client
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<Error> Run(long id, StringName method, params Variant[] args)
	{
		var client = await Instance();
		return client.RpcId(id, method, args);
	}


	/// <summary>
	/// More easily run a function directly to the server
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<Error> Run(Player player, StringName method, params Variant[] args)
	{
		return await Run(player.GetId(), method, args);
	}
}
