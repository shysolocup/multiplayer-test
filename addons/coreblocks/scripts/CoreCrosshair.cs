using Godot;


[OnClient]
public partial class CoreCrosshair : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override async void OnReady()
	{
		var coreGui = await CoreGui.Instance();

		var crosshair = coreGui.GetNode<TextureRect>("./crosshair");
		crosshair.Hide();

		if (coreGui.DefaultShiftlockGuiEnabled)
		{
			cameras.ShiftLockEnabled += crosshair.Show;
			cameras.ShiftLockDisabled += crosshair.Hide;	
		}
	}
}
