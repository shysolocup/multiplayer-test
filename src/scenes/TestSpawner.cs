using Godot;
using System;

public partial class TestSpawner : MultiplayerSpawner
{
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();

		await Game.WaitUntilConnected();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
