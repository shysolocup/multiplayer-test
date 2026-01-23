using Godot;

[GlobalClass, Icon("uid://7xxdvadqtbc3")]
public partial class GuiSystem : SingletonControl<GuiSystem>
{
	public static ShaderSystem Shaders { get; set; }
	public static CoreGui CoreGui { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();

		Shaders = await ShaderSystem.Instance();
		CoreGui = await CoreGui.Instance();
	}

	public override void _ExitTree()
    {
        base._ExitTree();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
