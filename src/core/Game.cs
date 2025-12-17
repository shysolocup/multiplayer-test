using Godot;
using System;

[Tool]
[GlobalClass, Icon("uid://boo8iw5pvoaa8")]
public partial class Game : Node
{

	public static Game Instance { get; private set; }

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
		{
			// Game.Save();
			GetTree().Quit();
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
