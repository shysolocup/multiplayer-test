using Godot;
using Godot.Collections;


[GlobalClass, Icon("uid://bougdaf1abff1")]
public partial class MultiplayerRemote : Node
{
    private static Players players;



    /// <summary>
    /// Event fired when a remote is received by the client sent from the server
    /// <para/> <c>server sends → client receives</c>
    /// <para/> @client
    /// </summary>
    [Signal]
    public delegate void OnClientEventEventHandler(Array<Variant> args);


    /// <summary>
    /// Event fired when a remote is received by the server sent from a client
    /// <para/> <c>client sends → server receives</c>
    /// <para/> @server
    /// </summary>
    [Signal]
    public delegate void OnServerEventEventHandler(Player sender, Array<Variant> args);
    

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    protected private void _fireClient(Array<Variant> args)
    {
        EmitSignalOnClientEvent(args);
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void FireClient(Player player, params Variant[] args)
    {
        Behavior.assert(Game.IsServer(), $"Cannot fire event {this} client to client.");
        
        var arr = new Array<Variant>(args);
        RpcId(player.GetPeerId(), MethodName._fireClient, arr);
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void FireAllClients(params Variant[] args)
    {
        Behavior.assert(Game.IsServer(), $"Cannot fire event {this} client to client.");

        var arr = new Array<Variant>(args);
        Rpc(MethodName._fireClient, arr);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void FireServer(params Variant[] args)
    {
        if (players is null) return; 
        
        var remoteId = Multiplayer.GetRemoteSenderId();
        var sender = players.GetPlayerById(remoteId);
        var arr = new Array<Variant>(args);

        EmitSignalOnServerEvent(sender, arr);
    }

    public override async void _Ready()
    {
        base._Ready();
        players = await Players.Instance();
    }
}