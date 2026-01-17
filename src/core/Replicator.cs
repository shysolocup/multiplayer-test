using System.Threading.Tasks;
using Core;
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


    public override async void _Ready()
    {
        base._Ready();

        PlayerSpawner = GetNode<MultiplayerSpawner>("./playerSpawner");
        CharacterSpawner = GetNode<MultiplayerSpawner>("./characterSpawner");


        var workspace = await Workspace.Instance();
        var players = await Players.Instance();
        var global = await GlobalStorage.Instance();
        var characters = await Characters.Instance();


        PlayerSpawner.SpawnFunction = Callable.From( (Godot.Collections.Array array) =>
        {
            var peerId = (long)array[0];
            var id = (string)array[1];

            GD.Print("spawn player");
            return players.MakePlayer(peerId, id);
        });

        CharacterSpawner.SpawnFunction = Callable.From((string name) =>
        {
            return characters.SpawnDummy(name);
        });


        await Game.WaitUntilConnected();
        
        var id = Server.GetPeer().UniqueId;
        
        if (Game.IsServer())
        {

            global.SetMultiplayerAuthority(id, recursive: true);
            players.SetMultiplayerAuthority(id, recursive: true);
            workspace.SetMultiplayerAuthority(id, recursive: true);

            GD.Print("set authority to server");

            players.PlayerJoined += OnJoin;
            OnJoin(Client.LocalPlayer);
        }
    }

    private static void OnJoin(Player player)
    {
        player.Spawned += character => {
            GD.Print("set character authority to client");
            character.SetMultiplayerAuthority((int)player.GetPeerId(), recursive: true);
        };

        if (player.GetCharacter() is Character chara)
        {
            GD.Print("set character authority to client");
            chara.SetMultiplayerAuthority((int)player.GetPeerId(), recursive: true);
        }
    }
}
