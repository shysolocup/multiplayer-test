using Godot;

[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{
    static public MultiplayerSpawner PlayerSpawner { get; set; }
    static public MultiplayerSpawner CharacterSpawner { get; set; }


    private Workspace workspace { get; set; }
    private Players players { get; set; }
    private GlobalStorage global { get; set; }
    private Characters characters { get; set; }
    private CameraSystem cameras { get; set; }
    private Client client { get; set; }


    public override async void _Ready()
    {
        base._Ready();

        var game = await Game.Instance();
        workspace = await Workspace.Instance();
        players = await Players.Instance();
        global = await GlobalStorage.Instance();
        cameras = await CameraSystem.Instance();
        characters = await Characters.Instance();
        client = await Client.Instance();

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

            global.SetMultiplayerAuthority(id, recursive: true);
            players.SetMultiplayerAuthority(id, recursive: true);
            workspace.SetMultiplayerAuthority(id, recursive: true);

            workspace.MakeReplicated(true);
            players.MakeReplicated(true);
            workspace.MakeReplicated(true);

            GD.Print("set authority to server");

            players.PlayerJoined += OnJoin;

            foreach (var player in players.GetPlayers())
            {
                OnJoin(player);
            }
        }
    }

    private async void OnJoin(Player player)
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
}
