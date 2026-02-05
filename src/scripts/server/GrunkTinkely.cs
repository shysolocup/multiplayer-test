
public partial class GrunkTinkely : ServerBehavior
{
	public override void OnReady()
	{
		print(isConnected());
		print(isServer());

		warn("AAAAAAAAAAAAAAAAAAAAAAA");

		print("You have been grunk tinkelyed!");
	}

}
