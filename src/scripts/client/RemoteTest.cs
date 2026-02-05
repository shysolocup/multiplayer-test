using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;


public partial class RemoteTest : ClientBehavior
{
    public void onClientEvent(Array<Variant> args)
    {
        print(args);
    }

    public override void OnReady()
    {
        var remote = global.GetEvent("test");

        remote.OnClientEvent += onClientEvent;

        task.Delay(1, async () =>
        {
            remote.FireServer("a", "b");
            // fuck the stupid async warning let me ball bro
            await Task.FromResult(0);
        });
    }
}