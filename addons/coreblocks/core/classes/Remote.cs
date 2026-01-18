using Godot;
using Godot.Collections;

public partial class Remote : Node
{
    [Signal]
    public delegate void OnClientEventEventHandler(Player player, Variant args);

    [Signal]
    public delegate void OnServerEventEventHandler(Variant args);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void FireClient(Player player, params Variant[] args)
    {
        var arr = new Array<Variant>(args);
        EmitSignalOnClientEvent(player, arr);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void FireAllClients(Player player, params Variant[] args)
    {
        var arr = new Array<Variant>(args);
        EmitSignalOnClientEvent(player, arr);
    }
}