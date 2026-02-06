using System;
using Godot.Collections;
using System.Threading.Tasks;
using Godot;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

[Tool]
[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{

    [Export]
    // if set to 0 it'll do it every frame
    public double ReplicationInterval = 0.1;

    private double repCounter = 0;

    static public MultiplayerSpawner PlayerSpawner { get; set; }
    static public MultiplayerSpawner CharacterSpawner { get; set; }

    protected private Godot.Collections.Dictionary<ulong, Dictionary> PropertyStore = [];
    protected private Godot.Collections.Array<ulong> ReplicatedObjects = [];

    public Godot.Collections.Array<string> ReplicatedFilter = [];
    protected private Godot.Collections.Array<string> PrivateFilter = [ "Character" ];


    private Workspace workspace { get; set; }
    private Players players { get; set; }
    private GlobalStorage global { get; set; }
    private Characters characters { get; set; }
    private CameraSystem cameras { get; set; }
    private Client client { get; set; }
    private Server server { get; set; }
    private Replicator replicator { get; set; }


    #region rep rpc calls


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	protected private void _replicate_to_clients(ulong selfId)
    {
        if (!IsInstanceIdValid(selfId)) return;

        var id = Multiplayer.GetRemoteSenderId();
        var obj = InstanceFromId(selfId);

        var authority = (obj is Node n ? n.GetMultiplayerAuthority() : server.GetHostPeerId()) == id;

        GD.Print("auto replicated to clients");

        if (authority && PropertyStore.TryGetValue(selfId, out var pairs))
        {
            Rpc(
                MethodName._replicate, 
                selfId, 
                pairs
            );   
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	protected private void _replicate_relay(ulong selfId)
    {
        if (!IsInstanceIdValid(selfId)) return;

        var id = Multiplayer.GetRemoteSenderId();
        var obj = InstanceFromId(selfId);

        var authority = (obj is Node n ? n.GetMultiplayerAuthority() : server.GetHostPeerId()) == id;

        GD.Print("relay replicated to clients");

        if (authority && PropertyStore.TryGetValue(selfId, out var pairs))
        {
            server.Invoke(
                this,
                MethodName._replicate, 
                selfId, 
                pairs
            );   
        }
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	protected private void _sync_to_server(ulong selfId)
	{
        if (!IsInstanceIdValid(selfId)) return;

        var id = Multiplayer.GetRemoteSenderId();
        var obj = InstanceFromId(selfId);

        var authority = (obj is Node n ? n.GetMultiplayerAuthority() : server.GetHostPeerId()) == id;

        GD.PushWarning("remote id: ", Multiplayer.GetRemoteSenderId(), " authority?: ", authority);

        if (authority && PropertyStore.TryGetValue(selfId, out var pairs))
        {
            RpcId(
                Multiplayer.GetRemoteSenderId(),
                MethodName._replicate, 
                selfId, 
                pairs
            );   
        }
	}

    /// <summary>
    /// clientside handling of replication
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    protected private async void _replicate(ulong selfId, Dictionary pairs)
    {
        if (Game.IsServer()) return;

        await Game.WaitUntilConnected();
        await Game.WaitUntilLoaded();

        GD.PushWarning(selfId);

        GD.Print("got here");

        if (IsInstanceIdValid(selfId) && InstanceFromId(selfId) is GodotObject obj)
        {
            Behavior.assert(
                Attribute.IsDefined(obj.GetType(), typeof(NotReplicatedAttribute)), 
                "it somehow made this far without being detected that it can't be replicated good job me"
            );

            foreach ((var key, var value) in this.Pairs(pairs))
            {
                GD.Print(key, value);
            }
        }
    }

    #endregion

    #region Sync

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public async Task<Error> Sync(GodotObject self, params string[] filter)
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");
        
        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;

        server ??= await Server.Instance();

        return server.Invoke(
            this, 
            MethodName._sync_to_server, 
            self.GetInstanceId()
        );
    }

    #endregion

    #region Replicate (For)


    /// <summary>
    /// 
    /// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public async Task<Error> Replicate(GodotObject self, params string[] filter)
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");

        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;

        client ??= await Client.Instance();

        return client.InvokeAll(
            this, MethodName._replicate_to_clients, 
            self.GetInstanceId()
        );
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public async Task<Error> ReplicateFor(long id, GodotObject self, params string[] filter)
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");

        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;
        
        client ??= await Client.Instance();

        return client.Invoke(
            id, 
            this, 
            MethodName._replicate_to_clients, 
            self.GetInstanceId()
        );
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public async Task<Error> ReplicateFor(Player player, GodotObject self, params string[] filter)
        => await ReplicateFor(
                player.GetPeerId(), 
                self, 
                filter
            );


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public async Task<Error> ReplicateFor(string playerId, GodotObject self, params string[] filter)
        => await ReplicateFor(
                players.GetPlayerById(playerId).GetPeerId(), 
                self, 
                filter
            );

    #endregion

    #region doReplicate

    protected async private void doReplicate(ulong key, GodotObject obj, bool recursive = true)
    {
        if (!IsInstanceIdValid(key) || Filtered(obj))
        {
            ReplicatedObjects.Remove(key);
            PropertyStore.Remove(key);
            return;
        }

        if (!PropertyStore.TryGetValue(key, out var oldProps))
        {
            PropertyStore[key] = obj.GetPropertyPairs();
            return;
        }

        var newProps = obj.GetPropertyPairs();
        var changed = false;

        foreach (var (kVar, newValue) in this.Pairs(newProps))
        {
            var k = kVar.AsString();

            if (oldProps.TryGetValue(k, out var oldValue) && !oldValue.Equals(newValue))
            {
                changed = true;
                break;
            }
        }

        if (changed) {
            GD.Print("changed");
            // await Replicate(obj);
            PropertyStore[key] = newProps;
        }

        if (obj is Node n && recursive)
        {
            foreach (var child in n.GetChildren())
            {
                doReplicate(child.GetInstanceId(), child);
            }
        }
    }

    #endregion

    #region Filtered

    private bool Filtered(GodotObject guh)
    {
        var value = Attribute.IsDefined(guh.GetType(), typeof(NotReplicatedAttribute));

        if (!value)
        {
            foreach (var f in ReplicatedFilter)
            {
                if (guh.IsClass(f)) {
                    value = true;
                    break;
                }
            }   
        }

        if (!value)
        {
            foreach (var f in PrivateFilter)
            {
                if (guh.IsClass(f)) {
                    value = true;
                    break;
                }
            }
        }

        return value;
    }

    #endregion

    #region HandleReplicator

    private void HandleReplicator()
    {
        if (Game.IsServer() && !Game.IsEditor() && Game.IsConnected() && ReplicatedObjects.Count >= 0)
        {
            foreach (var id in ReplicatedObjects)
            {
                if (!IsInstanceIdValid(id)) continue;

                var obj = InstanceFromId(id);
                
                doReplicate(id, obj);
            }
        }

        // await Task.Delay((int)(ReplicationInterval * 1000));
        // CallDeferred(MethodName.HandleReplicator);
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        
        repCounter += delta;

        if (repCounter > ReplicationInterval) {
            repCounter -= ReplicationInterval;
            HandleReplicator();
        }
        
    }


    #endregion

    public async Task WaitUntilReplicated(GodotObject self)
    {
        var id = self.GetInstanceId();

        while (!PropertyStore.ContainsKey(id) && !ReplicatedObjects.Contains(id))
        {
            await Task.Delay(10);
        }
    }

    #region StartReplicating

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async void StartReplicating(GodotObject self, bool recursive = false)
	{
        if (!Game.IsServer()) return;

        var filtered = Filtered(self);

        if (!filtered) {
            var id = self.GetInstanceId();
            GD.Print(id);

            ReplicatedObjects.Add(id);
            
            GD.Print(ReplicatedObjects);
        }
            

        if (self is Node n)
        {
            // I wish I had a better way to do this
            // but I can't think of one

            n.TreeExiting += () =>
            {
                PropertyStore.Remove(self.GetInstanceId());

                foreach (var child in n.GetChildren())
                    PropertyStore.Remove(child.GetInstanceId());
            };
        }
	}

    #endregion
    #region ready

    public override async void _Ready()
    {
        base._Ready();

        if (Game.IsEditor()) return;

        var game = await Game.Instance();
        workspace = await Workspace.Instance();
        players = await Players.Instance();
        global = await GlobalStorage.Instance();
        cameras = await CameraSystem.Instance();
        characters = await Characters.Instance();
        client = await Client.Instance();
        server = await Server.Instance();
        replicator = await Replicator.Instance();

        PlayerSpawner = GetNode<MultiplayerSpawner>("./playerSpawner");
        CharacterSpawner = GetNode<MultiplayerSpawner>("./characterSpawner");

        if (PlayerSpawner is null)
		{
			PlayerSpawner = new()
			{
				Name = "playerSpawner",
                SpawnPath = players.GetPath()
			};

			AddChild(PlayerSpawner);
			if (Engine.IsEditorHint()) {
				PlayerSpawner.Owner = game;
			}
		}

        if (CharacterSpawner is null)
		{
			CharacterSpawner = new()
			{
				Name = "characterSpawner",
                SpawnPath = characters.GetPath()
			};

			AddChild(CharacterSpawner);
			if (Engine.IsEditorHint()) {
				CharacterSpawner.Owner = game;
			}
		}


        if (Engine.IsEditorHint()) return;


        PlayerSpawner.SpawnFunction = Callable.From( (Godot.Collections.Array array) =>
        {
            var peerId = (long)array[0];
            var id = (string)array[1];

            GD.Print("spawn player");
            return players.MakePlayer(peerId, id);
        });

        CharacterSpawner.SpawnFunction = Callable.From((Godot.Collections.Array array) =>
        {
            var peerId = (int)array[0];
            var playerId = (string)array[1];
            var playerName = (string)array[2];

            return characters.SpawnDummy(peerId, playerId, playerName);
        });

        await Game.WaitUntilConnected();
        
        var id = Server.GetPeer().UniqueId;

        if (Game.IsServer())
        {   
            global.SetMultiplayerAuthority(id, true);
            players.SetMultiplayerAuthority(id, true);
            workspace.SetMultiplayerAuthority(id, true);
            replicator.SetMultiplayerAuthority(id, true);

            StartReplicating(workspace, true);
            StartReplicating(global, true);
            StartReplicating(players, true);
            StartReplicating(replicator, true);

            GD.Print("set authority to server");

            players.PlayerJoined += OnJoin;

            foreach (var player in players.GetPlayers())
            {
                OnJoin(player);
            }
        }

        CallDeferred(MethodName.HandleReplicator);
    }

    protected private async void OnJoin(Player player)
    {
        player.Spawned += async chara => {
            if (Game.IsServer())
            {
                GD.Print("set character authority to client");
                chara.SetMultiplayerAuthority((int)player.GetPeerId(), recursive: true);
            }

            var clientId = await client.GetId();
            var playerId = player.GetId();

            GD.Print($"player id: {playerId}, client id: {clientId}");

            if (playerId == clientId)
            {
                cameras.SetSubject(player);
            }
        };

        if (player.Character is Character chara)
        {
            if (Game.IsServer())
            {
                GD.Print("set character authority to client");
                chara.SetMultiplayerAuthority((int)player.GetPeerId(), recursive: true);
            }

            var clientId = await client.GetId();
            var playerId = player.GetId();

            GD.Print($"player id: {playerId}, client id: {clientId}");

            if (playerId == clientId)
            {
                cameras.SetSubject(player);
            }
        }
    }

    #endregion
}
