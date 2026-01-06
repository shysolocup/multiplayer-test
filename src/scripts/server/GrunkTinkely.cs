using Godot;

public partial class GrunkTinkely : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override async void OnReady()
	{
		print(workspace);

		foreach (Node guh in workspace.GetChildren())
		{
			print(guh.Name);
		}

		var target = await mouse.GetTarget<Node3D>();
	}
}
