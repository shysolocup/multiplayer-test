using Godot;

[OnServer]
public partial class GrunkTinkely : Behavior
{
	public override async void OnReady()
	{
		print(Replicator.IsConnected());
		warn("AAAAAAAAAAAAAAAAAAAAAAA");
	}

	[OnClient]
	public override void OnProcess(double delta)
	{
		base.OnProcess(delta);
	}

}
