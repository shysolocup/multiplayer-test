using Godot;
using System;

[GlobalClass, Icon("uid://rqxgol7tuknt")]
public partial class Client : Singleton<Client>
{
	private static Player _localPlayer;

	public static Player LocalPlayer
	{
		get => _localPlayer;
		private set
		{
			_localPlayer?.QueueFree();
			_localPlayer = value;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		// if ran from the server it'll say it loaded the host player
		var server = await Server.Instance();
		server.RpcId(1, Server.MethodName.PlayerLoaded);
	}
}
