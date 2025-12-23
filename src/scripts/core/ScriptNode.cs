using Godot;

[Tool]
[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class ScriptNode : Node
{

	#region signals

	[Signal] public delegate void EnabledChangedEventHandler(bool oldValue, bool newValue);
	[Signal] public delegate void SourceChangedEventHandler(bool oldValue, bool newValue);

	#endregion

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region file stuff


	[Export(PropertyHint.Enum, "C#,GDScript,C++,Lua,Python,Rust")]
	public int FileExtension = 0;


	private string DefaultFileContents
	{
		get => @"
using Godot;
using System;

public partial class " + Name + @" : "+ GetType().Name + @"
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}";	
	}

	#endregion

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region controls

	[Export] public bool Enabled {
		get => _enabled;
		set { 
			var old = _enabled;
			_enabled = value; 
			if (old != value) EmitSignal(SignalName.EnabledChanged, old, value); 
		}
	}

	[ExportToolButton("Execute")] 
	public Callable ExecuteCall => Callable.From(() => Execute());


	public void Execute(params object[] args) {
		var reloaded = ReloadSource().Obj;

		Node script = (Node)(
			(reloaded is CSharpScript cs) 
			? cs.New() 
			: (reloaded is GDScript gd) 
			? gd.New() 
			: nullvar
		);
		
		GetParent().AddChild(script);
		script.QueueFree();
	}

	#endregion

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region source controls

	[ExportGroup("Source Controls")]



	public string _source { get; set; }

	[Export(PropertyHint.File, "*.cs,*.gd,*.rs,*.cpp,*.lua,*.luau,*.py")] public string Source {
		get => _source;
		set {
			var old = _source;
			_source = value;
			if (old != value) EmitSignal(SignalName.SourceChanged, old, value); 
		}
	}



	// creates a new source file
	[ExportToolButton("Create New Source")]
	public Callable CreateFileCall => Callable.From(() =>
	{
		if (!Engine.IsEditorHint())
			return;

		ScriptNodeEditorBridge.RequestCreateSource(this);
	});

	// when the script is first created it'll automatically prompt the file the script can go in
	public override void _EnterTree()
	{
		if (_sourcePromptedOnStart || !Engine.IsEditorHint())
			return;

		ScriptNodeEditorBridge.RequestCreateSource(this);
		_sourcePromptedOnStart = true;
	}



	// button to reload the source file
	[ExportToolButton("Reload Source")] 
	private Callable ReloadSourceCall => Callable.From(ReloadSource);



	public Variant ReloadSource() => GD.Load(Source);

	#endregion


	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	[ExportGroup("Variables")]

	#region variables
	private bool _enabled = true;

	[Export] private bool _sourcePromptedOnStart = false;

	[Export] public Node SpawnedNode;
	private Variant nullvar = new();

	[Export] public NodePath SpawnedNodePath;

	#endregion


	#region methods


	public ScriptNode DetatchScript()
	{
		GetParent().RemoveChild(SpawnedNode);
		SpawnedNode.QueueFree();
		SpawnedNode = null;
		return this;
	}


	public Node AttachScript(string source = null)
	{
		Source = source ?? Source;
		
		if (SpawnedNode != null) {
			DetatchScript();
		}

		Variant Script = ReloadSource();

		if (Script.Obj is CSharpScript cs) SpawnedNode = (Node)cs.New();
		else if (Script.Obj is GDScript gd) SpawnedNode = (Node)gd.New();

		SpawnedNodePath = new NodePath($"{Name}Source");
		SpawnedNode.Name = SpawnedNodePath.ToString();
		
		GetParent().CallDeferred("add_child", SpawnedNode);

		return SpawnedNode;
	}


	private void HandleEnabledChange(bool oldVal, bool newVal)
	{
		if (newVal == true) {
			AttachScript();
		}
		else {
			if (SpawnedNode != null) {
				DetatchScript();
			}
		}
	}


	public void CreateFile(string path)
	{
		GD.Print("a");
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		
		if (file is null)
		{
			GD.PushError("Failed to create file at " + path);
			return;
		}

		file.StoreString(DefaultFileContents);
		file.Close();
		Source = path;
	}
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!Engine.IsEditorHint()) {
			EnabledChanged += HandleEnabledChange;
			SpawnedNodePath = new NodePath($"{Name}Source");

			Set("SpawnedNode", nullvar);

			if (Source != null && Enabled) {
				AttachScript();
			};
		}
	}


	#endregion
}
