using Godot;


public partial class RemoteTest : ClientBehavior
{
    public override void OnReady()
    {
        var remote = global.GetEvent("test");

        remote.OnClientEvent += (args) =>
        {
            GD.Print(args);
        };

        task.Delay(1, async () =>
        {
            remote.FireServer("a", "b");
        });
    }
}