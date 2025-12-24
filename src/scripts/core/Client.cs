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
	public override void _Ready()
	{
	}
}
