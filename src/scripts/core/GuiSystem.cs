using Godot;
using System;

[GlobalClass, Icon("uid://7xxdvadqtbc3")]
public partial class GuiSystem : SingletonControl<GuiSystem>
{
	public static ShaderSystem Shaders { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();

		Shaders = await ShaderSystem.Instance();
	}

	public override void _ExitTree()
    {
        base._ExitTree();

		// dotnet security
		Shaders = null;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
