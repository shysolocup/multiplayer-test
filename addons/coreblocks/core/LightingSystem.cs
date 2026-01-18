using Godot;
using Godot.Collections;


[Tool]
[GlobalClass, Icon("uid://caemrook4v2la")]
public partial class LightingSystem : Singleton3D<LightingSystem>
{
	[Signal] public delegate void LightingChangedEventHandler();
	[Signal] public delegate void LightingDisabledEventHandler();

	static public readonly string SceneDir = "res://src/assets/lightings";
	static private string DefaultScene = "default";

	public static Dictionary<string, PackedScene> SceneCache = new() {
		["default"] = ResourceLoader.Load<PackedScene>($"{SceneDir}/{DefaultScene}.tscn", "", ResourceLoader.CacheMode.Replace)
	};

	private PackedScene lighting = SceneCache[DefaultScene];

	/// <summary>
	/// default lighting it should go back to after doing temp lighting
	/// </summary> 
	public PackedScene TempLighting;

	[Export] public PackedScene Lighting {
		get => lighting;
		set {
			if (value != lighting && TempLighting is null) {
				lighting = value;
				ResetApply();
			}
		}
	}


	[ExportCategory("Sky")]

	private float timeofday = 12;

	[Export(PropertyHint.Range, "0.0,24.0,0.0001")]
	public float TargetTimeOfDay = 12;

	[Export(PropertyHint.Range, "0.0,24.0")]
	public float TimeOfDay
	{
		get => timeofday;
		set
		{
			DoTimeOfDay(-1);
			TargetTimeOfDay = value;
		}
	}

	[Export]
	public float SkyEasing = 20;

	private float MidnightDistance()
	{
		var time = Mathf.PosMod(TimeOfDay, 24);
		return Mathf.Min(time, 24 - time);
	}

	private float DaylightFactor()
	{
		float t = Mathf.PosMod(TimeOfDay, 24);

		float morning = Mathf.InverseLerp(0f, 7, t);
		float evening = 1 - Mathf.InverseLerp(17, 24, t);

		float daylight = Mathf.Min(morning, evening);
		daylight = Mathf.Clamp(daylight, 0, 1);

		daylight = Mathf.Pow(daylight, 25);

		return daylight;
	}

	private static float EaseOutCubic(float t)
	{
		return 1 - Mathf.Pow(1 - t, 3);
	}

	public void DoTimeOfDay(double delta)
	{
		if (delta != -1)
		{
			float diff = Mathf.Abs(TargetTimeOfDay - timeofday);
			float factor = Mathf.Clamp(diff / 12, 0f, 1);
			float easedSpeed = SkyEasing * EaseOutCubic(factor);

			timeofday = Mathf.MoveToward(
				timeofday,
				TargetTimeOfDay,
				easedSpeed * (float)delta
			);
		}

		if (World is null)
			EnsureWorld();
		if (Lights is null)
			EnsureLights();

		if (World is null || Lights is null) return;

		var angle = (TimeOfDay - 12) / 24 * float.Tau;

		float d = Mathf.Abs(TimeOfDay);
		d = Mathf.Min(d, 24 - d);

		// cumulus cloud fading
		// (cumulus is the big clouds)

		// thje threshold where it starts fading
		const float cumulusThreshold = 16;

		float maxDistance = Mathf.Min(
			Mathf.Abs(cumulusThreshold),
			24 - Mathf.Abs(cumulusThreshold)
		);

		float t = Mathf.Clamp(d / maxDistance, 0, 1);
		float baseValue = 1 - t;
		baseValue = Mathf.SmoothStep(0, 1, baseValue);

		float cumulus = Mathf.Lerp(0.7f, 10, baseValue);

		// cirrus cloud fading
		// (cirrus is the thing it looks kinda cook but weird)

		const float cirrusFadeStart = 13;
		const float cirrusFadeEnd = 11;

		float cirrusBase = (TimeOfDay >= cirrusFadeStart)
			? Mathf.InverseLerp(cirrusFadeStart, 20, TimeOfDay)
			: (TimeOfDay <= cirrusFadeEnd)
			? 1 - Mathf.InverseLerp(4, cirrusFadeEnd, TimeOfDay)
			: 0;

		cirrusBase = Mathf.Clamp(cirrusBase, 0, 1);
		cirrusBase = Mathf.SmoothStep(0, 1, cirrusBase);

		float cirrus = (1 - cirrusBase) * 0.2f;


		SkyMaterial.SetShaderParameter("clouds_alpha_upper_bound", cumulus);
		SkyMaterial.SetShaderParameter("cirrus_opacity", cirrus);

		var sgugh = Lights.GetNode("./sunlight");

		if (sgugh is Sunlight sunlight)
		{
			float factor = DaylightFactor();
			sunlight.LightEnergy = sunlight.BaseEnergy * factor;

			float warmth = 1 - factor;

			// Lerp from normal sun color to sunset color
			sunlight.LightColor = sunlight.BaseColor.Lerp(Colors.OrangeRed, warmth);
		}

		Rotation = new Vector3(Rotation.X, Rotation.Y, angle);
	}


	public override void _Process(double delta)
	{
		base._Process(delta);

		DoTimeOfDay(delta);
	}


	public WorldEnvironment EnsureWorld()
	{
		if (World is null)
		{
			foreach (var node in GetChildren())
			{
				if (node is WorldEnvironment world)
					World = world;
					return World;
			}	
		}
		else 
			return World;

		return null;
	}

	public Marker3D EnsureLights()
	{
		if (Lights is null)
		{
			foreach (var node in GetChildren())
			{
				if (node is Marker3D lights)
					Lights = lights;
					return Lights;
			}	
		}
		else 
			return Lights;

		return null;
	}

	[Export]
	public ShaderMaterial SkyMaterial { 
		get => EnsureWorld()?.Environment.Sky.SkyMaterial is ShaderMaterial shader ? shader : null; 
		set
		{	
			EnsureWorld();

			if (World is not null)
			{
				World.Environment.Sky.SkyMaterial = value; 		
			}
		}
	}



	public Node CurrentLighting;
	public WorldEnvironment SceneWorld;
	public Marker3D SceneLights;

	[Export]
	public WorldEnvironment World;
	[Export]
	public Marker3D Lights;

	[ExportToolButton("Reset / Apply")] 
	private Callable ResetCall => Callable.From(_resetCall);
	private void _resetCall() => ResetApply();

	private Callable DayCall => Callable.From(_dayCall);
	private void _dayCall()
	{
		
	}

	public bool LightingIs(string name) => Lighting is PackedScene light && light.ResourcePath.Contains(name);
	public bool TempLightingIs(string name) => TempLighting is PackedScene temp && temp.ResourcePath.Contains(name);

	public LightingSystem DisposeLightings()
	{
		World?.QueueFree();
		Lights?.QueueFree();
		World = null;
		Lights = null;
		return this;
	}

	public LightingSystem ResetApply(PackedScene lighting = null) 
	{
		if (Visible && TempLighting is null) {
			lighting ??= Lighting;

			if (IsInstanceValid(lighting)) {

				this.ClearChildren();

				using Node CurrentLighting = lighting.Instantiate();

				if (CurrentLighting is null || !IsInstanceValid(CurrentLighting)) {
					// DebugConsole.LogError($"LightingError: failed to instantiate lighting scene \"{lighting.ResourcePath}\"");
					return this;
				}

				SceneWorld?.QueueFree();
				SceneLights?.QueueFree();
				
				SceneWorld = null;
				SceneLights = null;

				foreach (var node in CurrentLighting.GetChildren())
				{
					if (node is WorldEnvironment world)
						SceneWorld = world;
					
					if (node is Marker3D marker)
						SceneLights = marker;
				}

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Lights = SceneLights?.Duplicate<Marker3D>();

				if (World is not null && IsInstanceValid(World)) {
					GD.Print($"Set World to {World}");
					AddChild(World);
				}

				if (Lights is not null && IsInstanceValid(Lights)) {
					GD.Print($"Added lights {Lights}");
					AddChild(Lights);
				}

				EmitSignalLightingChanged();
			}
			else {
				// DebugConsole.LogError($"LightingError: invalid lighting scene \"{lighting.ResourcePath}\"");
			}
		}

		return this;
	}

	public static PackedScene LoadFromScene(string scene, bool useCache = true) 
	{
		if (ResourceLoader.Exists($"{SceneDir}/{scene}.tscn")) {
			PackedScene ps = (useCache && SceneCache.TryGetValue(scene, out PackedScene value)) ? value : ResourceLoader.Load<PackedScene>($"{SceneDir}/{scene}.tscn", "", ResourceLoader.CacheMode.Replace);
			if (!SceneCache.ContainsKey(scene) || !useCache) SceneCache[scene] = ps;
			return ps;
		}
		else {
			// DebugConsole.LogError($"LightingError: cannot find or failed to load scene \"{SceneDir}/{scene}.tscn\"");
			return null;
		}
	}

	public LightingSystem LoadAndSetFromScene(string scene, bool useCache = true)
	{
		if (LoadFromScene(scene, useCache) is PackedScene packed) {
			Lighting = packed;
		}
		return this;
	}

	public LightingSystem SetTempLighting(string scene, bool useCache = true)
	{
		if (Visible)
		{
			TempLighting = LoadFromScene(scene, useCache);

			if (IsInstanceValid(TempLighting))
			{

				this.ClearChildren();

				using Node CurrentLighting = TempLighting.Instantiate();

				if (CurrentLighting is null || !IsInstanceValid(CurrentLighting))
				{
					// DebugConsole.LogError($"LightingError: failed to instantiate lighting scene \"{TempLighting.ResourcePath}\"");
					return this;
				}

				SceneWorld?.QueueFree();
				SceneLights?.QueueFree();

				SceneWorld = null;
				SceneLights = null;

				foreach (var node in CurrentLighting.GetChildren())
				{
					if (node is WorldEnvironment world)
						SceneWorld ??= world;
					
					if (node is Marker3D marker)
						SceneLights ??= marker;
				}

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Lights = SceneLights?.Duplicate<Marker3D>();

				if (World is not null && IsInstanceValid(World))
				{
					GD.Print($"Set World to {World}");
					AddChild(World);
				}

				if (Lights is not null && IsInstanceValid(Lights))
				{
					GD.Print($"Added lights {Lights}");
					AddChild(Lights);
				}

				EmitSignalLightingChanged();
			}
			else
			{
				// DebugConsole.LogError($"LightingError: invalid lighting scene \"{TempLighting.ResourcePath}\"");
			}
		}

		return this;
	}

	public LightingSystem SetFromScene(PackedScene scene)
	{
		lighting = scene;
		return this;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
	}

	public void OnVisibilityChanged() 
	{
		if (Lights is not null) Lights.Visible = Visible;

		if (Visible) {
			World?.QueueFree();
			World = SceneWorld?.Duplicate<WorldEnvironment>();
			if (World is not null) AddChild(World);
		}
		else {
			World?.QueueFree();
			World = null;
		}

		EmitSignalLightingDisabled();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		
		if (Engine.IsEditorHint())
		{
			ResetApply();

			if (World is null || Lights is null)
			{
				foreach (var node in GetChildren())
				{
					if (node is WorldEnvironment world)
						World ??= world;
					
					if (node is Marker3D marker)
						Lights ??= marker;
				}	
			}

			if (World is not null)
				DoTimeOfDay(-1);
		}
	}

	public override async void _Ready()
	{
		base._Ready();
		
		Connect(Node3D.SignalName.VisibilityChanged, new Callable(this, MethodName.OnVisibilityChanged));
		
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		if (Visible) ResetApply();
	}
}
