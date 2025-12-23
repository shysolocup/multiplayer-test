using Godot;
using System;

[GlobalClass, Icon("uid://7xxdvadqtbc3")]
public partial class GuiSystem : SingletonControl<GuiSystem>
{
	public ShaderSystem Shaders { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
