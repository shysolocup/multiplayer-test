using Godot;
using System;


public partial class Crosshair : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override async void OnReady()
	{
		var crosshair = gui.GetNode<TextureRect>("./core/crosshair");
		crosshair.Hide();

		cameras.ShiftLockEnabled += crosshair.Show;
		cameras.ShiftLockDisabled += crosshair.Hide;
	}
}
