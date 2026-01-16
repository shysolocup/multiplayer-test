using System.Threading.Tasks;
using Core;
using Godot;

[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{
    static public MultiplayerSpawner PlayerSpawner { get; set; }
    static public MultiplayerSpawner CharacterSpawner { get; set; }

    private Players players { get; set; }
    private Characters characters { get; set; }


    private Player PlayerSpawn(int peerId, string id)
    {
        var player = (Player)players.StarterPlayer.Instantiate();
            player.ProcessMode = ProcessModeEnum.Inherit;
            player.SetPeerId(peerId);
            player.SetPlayerId(id);
            player.SetPlayerName($"player:${id}");

        return player;
    }

    private Callable PlayerSpawnCall => new(this, MethodName.PlayerSpawn);


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public override async void _Ready()
    {
        base._Ready();

        players = await Players.Instance();
        characters = await Characters.Instance();

        PlayerSpawner = GetNode<MultiplayerSpawner>("./playerSpawner");
        CharacterSpawner = GetNode<MultiplayerSpawner>("./characterSpawner");

        PlayerSpawner.SpawnFunction = PlayerSpawnCall;
        // CharacterSpawner.SpawnFunction = SpawnCall;

        if (Game.IsServer())
        {
            var workspace = await Workspace.Instance();
            var global = await GlobalStorage.Instance();

            SetAuthorityRecursive(workspace);
            SetAuthorityRecursive(global);
        }
    }


    private void SetAuthorityRecursive(Node node)
    {
        node.SetMultiplayerAuthority(1);

        foreach (Node child in node.GetChildren())
            SetAuthorityRecursive(child);
    }
}
