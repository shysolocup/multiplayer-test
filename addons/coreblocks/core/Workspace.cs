using Godot;

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
	public AudioSystem Audios;
	
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
			Audios = GetNode<AudioSystem>("./audios");
		}
	}

	public static uint GetCollisionGroupValue(string layerName)
	{
		for (int i = 1; i <= 32; i++)
		{
			var setting = ProjectSettings.GetSetting($"layer_names/3d_physics/layer_{i}");

			if (setting.VariantType == Variant.Type.String && setting.AsString() == layerName)
			{
				return 1u << (i - 1);
			}
		}

		return 0;
	}

	public uint GetCollisionGroupValue(string layerName, object _ = null)
	{
		for (int i = 1; i <= 32; i++)
		{
			var setting = ProjectSettings.GetSetting($"layer_names/3d_physics/layer_{i}");
			
			if (setting.VariantType == Variant.Type.String && setting.AsString() == layerName)
			{
				return 1u << (i - 1);
			}
		}

		return 0;
	}

	static void AddCollisionGroup(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return;

		for (int i = 1; i <= 32; i++)
		{
			var key = $"layer_names/3d_physics/layer_{i}";
			var existing = ProjectSettings.GetSetting(key).AsString();

			if (string.IsNullOrEmpty(existing))
			{
				ProjectSettings.SetSetting(key, name);
				ProjectSettings.Save();
				return;
			}
		}

		GD.PushWarning("No free collision layer slots remaining.");
	}
}
