using Godot;


[GlobalClass, Icon("uid://dj1gftejreec6")]
public partial class Player : Node
{
    [Export] public string PlayerName { get; set; } = "Player";
    [Export] public string PlayerId { get; set; }

    [Export] public Character Character { get; set; }

    [Export] public GuiSystem Gui { get; set; }
    [Export] public Camera3D Camera { get; set; }

    public Character Spawn()
    {
        var character = new Character();
        character.Player = this;
        return character;
    }

    public override void _Ready()
    {
        base._Ready();

        // init 

    }
}