using Godot;
using System;

[Tool]
public partial class GrunkTinkely : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		base.OnReady();

		Print(workspace);

		foreach (Node guh in workspace.GetChildren())
		{
			Print(guh.Name);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void OnProcess(double delta)
	{
		base.OnProcess(delta);
	}
}
