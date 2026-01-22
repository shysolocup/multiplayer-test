
public partial class GrunkTinkely : ServerBehavior
{
	public override async void OnReady()
	{
		print(game.IsConnected());
		print(isServer());

		warn("AAAAAAAAAAAAAAAAAAAAAAA");
	}

}
