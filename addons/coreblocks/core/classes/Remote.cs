using System;
using Godot;
using Godot.Collections;

public partial class Remote : GodotObject
{
    [Signal]
    public delegate void OnServerEventHandler(Player sender, Variant args);
    
    [Signal]
    public delegate void OnClientEventHandler(Variant args);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    private void _fireClient(Array<Variant> args)
    {
        EmitSignalOnClient(args);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void _fireServer(int sender, Array<Variant> args)
    {
        var player = Game.Systems.Players.GetPlayerById(sender);
        EmitSignalOnServer(player, args);
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public Error FireClient(Player player, params Variant[] args)
    {
        Behavior.assert(Game.IsServer(), "how did you get here");

        return Game.Systems.Client.Invoke(
            player.GetPeerId(), 
            this, 
            MethodName._fireClient, 
            Table.Pack(args) // pack the args to a variant
        );
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public Error FireAllClients(params Variant[] args)
    {
        Behavior.assert(Game.IsServer(), "how did you get here");

        return Game.Systems.Client.InvokeAll(
            this, 
            MethodName._fireClient, 
            Table.Pack(args) // pack the args to a variant
        );
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public Error FireServer(params Variant[] args)
    {
        return Game.Systems.Server.Invoke(
            this,
            MethodName._fireServer,
            Client.LocalPlayer.GetPeerId(), // send the sender
            Table.Pack(args) // pack the args to a variant
        );
    }
}