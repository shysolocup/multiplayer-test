using Godot;
using System;

[Tool]
[GlobalClass, Icon("uid://cr2l07lnbg74c")]
public partial class Workspace : Singleton3D<Workspace>
{
	private Marker3D _spawn;

	[Export]
	public Marker3D Spawn
	{
		get => _spawn ?? GetNode<Marker3D>("./spawn");
		set => _spawn = value;
	}

	[Export]
	public float Gravity
	{
		get => (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		set => ProjectSettings.SetSetting("physics/3d/default_gravity", value);
	}
	
	[ExportToolButton("Reset Lights & Map")] 
	private Callable ResetCall => Callable.From(_resetCall);
	private void _resetCall()
	{
		Lighting ??= GetNode<LightingSystem>("./lighting");
		Map ??= GetNode<MapSystem>("./map");

		Map.ResetApply();
		Lighting.ResetApply();
	}
	
	
	public LightingSystem Lighting;
	public MapSystem Map;
	public Characters Characters;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		
		Spawn ??= GetNode<Marker3D>("./spawn");

		if (!Engine.IsEditorHint())
		{
			Lighting = GetNode<LightingSystem>("./lighting");
			Map = GetNode<MapSystem>("./map");
			Characters = GetNode<Characters>("./characters");	
		}
	}
}
