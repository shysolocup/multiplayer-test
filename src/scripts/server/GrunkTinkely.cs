using Godot;

[OnServer]
public partial class GrunkTinkely : Behavior
{
	public override async void OnReady()
	{
		print(game.IsConnected());
		print(isServer());

		warn("AAAAAAAAAAAAAAAAAAAAAAA");
	}

}
