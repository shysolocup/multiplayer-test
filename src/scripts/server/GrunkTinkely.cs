using Godot;
using System;

public partial class GrunkTinkely : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override async void OnReady()
	{
		if (isServer())
		{
			print(workspace);

			foreach (Node guh in workspace.GetChildren())
			{
				print(guh.Name);
			}

			var target = await mouse.GetTarget<Node3D>();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void OnProcess(double delta)
	{
	}
}
