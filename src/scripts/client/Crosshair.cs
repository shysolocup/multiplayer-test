using Godot;
using System;

// check the hover text for the base Behavior class for method names
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
