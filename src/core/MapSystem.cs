using Godot;
using Godot.Collections;


[Tool]
[GlobalClass, Icon("uid://bu4xjum6vtr11")]
public partial class MapSystem : Singleton3D<MapSystem>
{
	[Signal] public delegate void MapChangedEventHandler();

	static public readonly string SceneDir = "res://src/maps";
	static public string DefaultScene = "default";

	public static Dictionary<string, PackedScene> SceneCache = new() {
		["default"] = ResourceLoader.Load<PackedScene>($"{SceneDir}/{DefaultScene}.tscn", "", ResourceLoader.CacheMode.Replace)
	};

	private PackedScene mapScene = SceneCache[DefaultScene];

	[Export] public PackedScene MapScene {
		get => mapScene;
		set {
			if (value != mapScene) {
				mapScene = value;
				ResetApply();
			}
		}
	}

	[Export]
	public Node Map;

	[ExportToolButton("Reset / Apply")] 
	private Callable ResetCall => Callable.From(_resetCall);
	private void _resetCall() => ResetApply();

	public MapSystem DisposeMap() 
	{
		foreach (Node child in GetChildren()) if (IsInstanceValid(child)) child?.QueueFree();
		if (IsInstanceValid(Map)) Map?.QueueFree();
		Map = null;

		return this;
	}

	public MapSystem ResetApply(PackedScene map = null) 
	{
		map ??= MapScene;

		if (IsInstanceValid(map)) {

			DisposeMap();

			Map = map.Instantiate();

			if (Map is not null && IsInstanceValid(Map)) {
				GD.Print($"Set Map to {Map}");

				AddChild(Map);

				EmitSignalMapChanged();
			}
			else {
				//DebugConsole.LogError($"MapError: failed to instantiate map scene \"{map.ResourcePath}\"");
			}
		}
		else {
			//DebugConsole.LogError($"MapError: invalid map scene \"{map.ResourcePath}\"");
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
			//DebugConsole.LogError($"MapError: cannot find or failed to load scene \"{SceneDir}/{scene}.tscn\"");
			return null;
		}
	}

	public MapSystem LoadAndSetFromScene(string scene, bool useCache = true)
	{
		if (LoadFromScene(scene, useCache) is PackedScene packed) {
			MapScene = packed;
		}
		return this;
	}

	public MapSystem SetFromScene(PackedScene scene)
	{
		MapScene = scene;
		return this;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		DisposeMap();
   	}
	
	public override void _EnterTree()
	{
		base._EnterTree();

		if (Engine.IsEditorHint()) {
			ResetApply();

			Map ??= GetChild(0);
		}
	}

	public override async void _Ready()
	{
		base._Ready();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		ResetApply();
	}
}
