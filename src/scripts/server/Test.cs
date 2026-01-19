using Godot;

[OnServer]
public partial class Test : Behavior
{
    public override void OnReady()
    {
        var remote = global.GetEvent("test");

        remote.OnServerEvent += (sender, args) =>
        {
            GD.Print(sender, args);
        };

        task.Delay(2, async () =>
        {
            remote.FireAllClients("a", "b");
        });
    }
}