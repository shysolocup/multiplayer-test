using Godot;
using System;

[Tool]
[GlobalClass, Icon("uid://cr2l07lnbg74c")]
public partial class World : Node3D
{
	[Export]
	public float Gravity
	{
		get => (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		set => ProjectSettings.SetSetting("physics/3d/default_gravity", value);
	}
	
	[ExportToolButton("Reset Lights & Map")] 
	public Callable ResetCall => Callable.From(() => {
		Lighting ??= GetNode<LightingSystem>("./lighting");
		Map ??= GetNode<MapSystem>("./map");

		Map.ResetApply();
		Lighting.ResetApply();
	});
	
	
	public LightingSystem Lighting;
	public MapSystem Map;
	public Characters Characters;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Lighting = GetNode<LightingSystem>("./lighting");
		Map = GetNode<MapSystem>("./map");
		Characters = GetNode<Characters>("./characters");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
