using Godot;
using Godot.Collections;


[Tool]
[GlobalClass, Icon("uid://caemrook4v2la")]
public partial class LightingSystem : Singleton3D<LightingSystem>
{
	[Signal] public delegate void LightingChangedEventHandler();
	[Signal] public delegate void LightingDisabledEventHandler();

	static public readonly string SceneDir = "res://src/lightings";
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

	[Export(PropertyHint.Enum, "None:1,EditorOnly:2,RuntimeOnly:3")] public int AutoVisibility = 1;

	public Node CurrentLighting;
	public WorldEnvironment SceneWorld;
	public DirectionalLight3D SceneSun;

	public WorldEnvironment World;
	public DirectionalLight3D Sun;

	[ExportToolButton("Reset / Apply")] 
	private Callable ResetCall => Callable.From(_resetCall);
	private void _resetCall() => ResetApply();

	public bool LightingIs(string name) => Lighting is PackedScene light && light.ResourcePath.Contains(name);
	public bool TempLightingIs(string name) => TempLighting is PackedScene temp && temp.ResourcePath.Contains(name);

	public LightingSystem DisposeLightings()
	{
		World?.QueueFree();
		Sun?.QueueFree();
		World = null;
		Sun = null;
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
				SceneSun?.QueueFree();
				
				SceneWorld = null;
				SceneSun = null;

				SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
				SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Sun = SceneSun?.Duplicate<DirectionalLight3D>();

				if (World is not null && IsInstanceValid(World)) {
					GD.Print($"Set World to {World}");
					AddChild(World);
				}

				if (Sun is not null && IsInstanceValid(Sun)) {
					GD.Print($"Set Sun to {Sun}");
					AddChild(Sun);
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
				SceneSun?.QueueFree();

				SceneWorld = null;
				SceneSun = null;

				SceneWorld = CurrentLighting.FindChild<WorldEnvironment>("T");
				SceneSun = CurrentLighting.FindChild<DirectionalLight3D>("T");

				World = SceneWorld?.Duplicate<WorldEnvironment>();
				Sun = SceneSun?.Duplicate<DirectionalLight3D>();

				if (World is not null && IsInstanceValid(World))
				{
					GD.Print($"Set World to {World}");
					AddChild(World);
				}

				if (Sun is not null && IsInstanceValid(Sun))
				{
					GD.Print($"Set Sun to {Sun}");
					AddChild(Sun);
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
		DisposeLightings();
   	}

	public void OnVisibilityChanged() 
	{
		if (Sun is not null) Sun.Visible = Visible;

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

	public override async void _Ready()
	{
		base._Ready();
		_ = Connect(SignalName.VisibilityChanged, new Callable(this, MethodName.OnVisibilityChanged));
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		switch(AutoVisibility) {
			case 2: { // editor only
				if (Engine.IsEditorHint()) {
					Visible = true;
				}
				break;
			}

			case 3: { // runtime only
				if (!Engine.IsEditorHint()) {
					Visible = true;
				}
				break;
			}
		}

		if (Visible) ResetApply();
	}
}
