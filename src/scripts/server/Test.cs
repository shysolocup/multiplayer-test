using System;
using Godot.Collections;
using Godot;
using System.Threading.Tasks;


public partial class Test : ServerBehavior
{
    private void onServerEvent(Player sender, Array<Variant> args)
    {
        print(sender, args);
    }

    public override void OnReady()
    {
        var remote = global.GetEvent("test");

        remote.OnServerEvent += onServerEvent;

        task.Delay(2, async () =>
        {
            remote.FireAllClients("a", "b");

            // fuck the stupid async warning let me ball bro
            await Task.FromResult(0);
        });
    }
}