using System.Threading.Tasks;
using Godot;


[GlobalClass, Icon("uid://dj1gftejreec6")]
public partial class Player : Node
{
    /// <summary>
    /// Event for when the character is created
    /// </summary>
    /// <param name="character"></param>
    [Signal]
    public delegate void SpawnedEventHandler(Character character);

    [Export] public string PlayerName { get; set; } = "Player";
    [Export] public int PlayerId { get; set; }

    [Export] public Character Character { get; set; }

    [Export] public GuiSystem Gui { get; set; }
    [Export] public Camera3D Camera { get; set; }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public override async void _Ready()
    {
        base._Ready();

        if (GetParent() is not Players)
        {
            var players = await Players.Instance();
            players.AddChild(this);
        }

        await Characters.Spawn(this);

        // init 

    }
}