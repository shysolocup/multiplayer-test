using System;
using Godot.Collections;
using System.Threading.Tasks;
using Godot;

[Tool]
[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{
    static public MultiplayerSpawner PlayerSpawner { get; set; }
    static public MultiplayerSpawner CharacterSpawner { get; set; }

    protected private Dictionary<ulong, Dictionary> PropertyStore = [];
    protected private Array<GodotObject> ReplicatedNodes = [];


    private Workspace workspace { get; set; }
    private Players players { get; set; }
    private GlobalStorage global { get; set; }
    private Characters characters { get; set; }
    private CameraSystem cameras { get; set; }
    private Client client { get; set; }
    private Server server { get; set; }
    private Replicator replicator { get; set; }


    #region auto replication


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	protected private void _replicate_to_clients(ulong selfId)
    {
        GD.Print("auto replicated to clients");

        if (IsInstanceIdValid(selfId) && PropertyStore.TryGetValue(selfId, out var pairs))
        {
            Rpc(
                MethodName._replicate, 
                selfId, 
                pairs
            );   
        }
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	protected private void _replicate_from_server(ulong selfId)
	{
        if (Game.IsClient() || !IsInstanceIdValid(selfId)) return;

        GD.PushWarning("remote id: ", Multiplayer.GetRemoteSenderId());

        if (PropertyStore.TryGetValue(selfId, out var pairs))
        {
            RpcId(
                Multiplayer.GetRemoteSenderId(),
                MethodName._replicate, 
                selfId, 
                pairs
            );   
        }
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    protected private void _replicate(ulong selfId, Dictionary pairs)
    {
        if (Game.IsServer()) return;

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


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public async Task<Error> Sync(GodotObject self, 
        bool recursive = true, 
        Array<StringName> properties = null
    )
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");
        
        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;

        server ??= await Server.Instance();
        return server.Invoke(this, MethodName._replicate_from_server, self.GetInstanceId());
    }


    /// <summary>
    /// 
    /// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async Task<Error> Replicate(GodotObject self)
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");

        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;

        client ??= await Client.Instance();
        return client.InvokeAll(this, MethodName._replicate_to_clients, self.GetInstanceId());
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async Task<Error> ReplicateFor(long id, GodotObject self)
	{
		Behavior.assert(Game.IsConnected(), "not connected yet");

        if (Attribute.IsDefined(self.GetType(), typeof(NotReplicatedAttribute))) return Error.Unauthorized;
        
        client ??= await Client.Instance();
        return client.Invoke(id, this, MethodName._replicate_to_clients, self.GetInstanceId());
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async Task<Error> ReplicateFor(Player player, GodotObject self)
        => await ReplicateFor(player.GetPeerId(), self);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async Task<Error> ReplicateFor(string playerId, GodotObject self)
        => await ReplicateFor(players.GetPlayerById(playerId).GetPeerId(), self);


    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Game.IsEditor() || !Game.IsConnected() || Game.IsClient() || ReplicatedNodes.Count == 0 || PropertyStore.Count == 0) return;

        foreach (var node in ReplicatedNodes)
        {
            var id = node.GetInstanceId();
            
            if (PropertyStore.TryGetValue(id, out var nodeProperties))
            {
                var newProps = node.GetPropertyPairs();
            }
        }
    }



    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public async void StartReplicating(GodotObject self, 
        bool recursive = false, 
        Array<StringName> properties = null,
        Array<Node> filter = null
    )
	{
        if (Game.IsClient()) return;

        var type = self.GetType();
        
        if (Attribute.IsDefined(type, typeof(NotReplicatedAttribute))) return;

        var pairs = self.GetPropertyPairs();
		PropertyStore[self.GetInstanceId()] = pairs;


		if (self is Node n)
		{
            n.TreeExiting += () =>
            {
                PropertyStore.Remove(self.GetInstanceId());

                foreach (var child in n.GetChildren())
                    PropertyStore.Remove(child.GetInstanceId());
            };

            if (recursive)
            {
                foreach (var child in n.GetChildren())
                {
                    if (!child.IsNodeReady()) 
                        await n.ToSignal(child, Node.SignalName.Ready);

                    StartReplicating(child, recursive: recursive);
                }

                // when a child is created and it's recursively replicating then it'll also create and replicate the child
                n.ChildEnteredTree += async (Node child) =>
                {
                    if (!child.IsNodeReady()) 
                        await n.ToSignal(child, Node.SignalName.Ready);

                    StartReplicating(child, recursive: recursive);
                };   
            }

            await Replicate(n);
        }

		else
		{
			self.PropertyListChanged += async () =>
			{
				await Replicate(self);
			};

            await Replicate(self);
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

            workspace.StartReplicating(true);
            players.StartReplicating(true);
            workspace.StartReplicating(true);
            replicator.StartReplicating(true);

            GD.Print("set authority to server");

            players.PlayerJoined += OnJoin;

            foreach (var player in players.GetPlayers())
            {
                OnJoin(player);
            }
        }
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
