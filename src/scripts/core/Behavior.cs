using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;


[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class Behavior : Node
{
	public bool ScriptReady = false;

	#region overridable events

	public bool isActionPressed(StringName actionName) => Input.IsActionPressed(actionName);
	public bool wasActionJustReleased(StringName actionName) => Input.IsActionJustReleased(actionName);
	public bool wasActionJustPressed(StringName actionName) => Input.IsActionJustPressed(actionName);

	public bool isKeyPressed(Key key) => Input.IsKeyPressed(key);

	public virtual void OnKeyPressed(InputEventKey key)
	{
	}

	public virtual void OnKeyReleased(InputEventKey key)
	{
	}

	public virtual void OnTabbedIn()
	{
	}

	public virtual void OnTabbedOut()
	{
	}

	/// <summary>
	/// Overridable event function that only runs on the server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public virtual void OnServer()
	{   
	}

	/// <summary>
	/// Overridable event function that only runs on the client
	/// </summary>
	public virtual void OnClient()
	{
	}

	/// <summary>
	/// Overridable event function for when the script is being created
	/// </summary>
	public virtual void OnCreation()
	{
	}

	/// <summary>
	/// Overridable event function for when the script is deleting
	/// </summary>
	public virtual void OnDeletion()
	{
	}

	/// <summary>
	/// Overridable event function for when the script and its dependencies are ready, not the player. 
	/// <para/> For when the player is fully loaded and ready use <c>OnLoaded()</c>
	/// </summary>
	public virtual void OnReady()
	{
	}

	/// <summary>
	/// Overridable event function ran every frame
	/// </summary>
	public virtual void OnProcess(double delta)
	{
	}

	/// <summary>
	/// Overridable event function for physics calculations
	/// </summary>
	public virtual void OnPhysics(double delta)
	{
	}

	/// <summary>
	/// Overridable event function for when the player has fully loaded
	/// /// <para/>@client
	/// </summary>
	public virtual void OnLoaded()
	{
	}

	#endregion
	#region dependencies

	public Client client;
	public AudioSystem audios;
	public CameraSystem cameras;
	public Characters characters;
	public Game game;
	public GlobalStorage global;
	public LightingSystem lighting;
	public MapSystem map;
	public Players players;
	public ClientScriptSystem clientScripts;
	public Workspace workspace;
	public GuiSystem gui;
	public ShaderSystem shaders;
	public Mouse mouse;

	public override async void _Ready()
	{
		if (Enabled)
		{
			ProcessMode = ProcessModeEnum.Inherit;   
		}
		else
		{
			ProcessMode = ProcessModeEnum.Disabled;
		}

		if (isEditor()) return;

		client = await Client.Instance();
		audios = await AudioSystem.Instance();
		cameras = await CameraSystem.Instance();
		characters = await Characters.Instance();
		game = await Game.Instance();
		global = await GlobalStorage.Instance();
		lighting = await LightingSystem.Instance();
		map = await MapSystem.Instance();
		players = await Players.Instance();
		clientScripts = await ClientScriptSystem.Instance();
		workspace = await Workspace.Instance();
		gui = await GuiSystem.Instance();
		shaders = await ShaderSystem.Instance();
		mouse = await Mouse.Instance();

		// it's specifically just in this order so ready fires before processes
		// this might lead to issues but we ball
		OnReady();

		if (isServer())
		{
			OnServer();
		}
		else
		{
			OnClient();
		}
		
		ScriptReady = true;
	}

	#endregion

	#region event handlers

	public override void _Notification(int what)
	{
		base._Notification(what);

		if (what == NotificationApplicationFocusIn)
		{
			OnTabbedIn();
		}
		else if (what == NotificationApplicationFocusOut)
		{
			OnTabbedOut();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (@event is InputEventKey key)
		{
			if (key.Pressed) OnKeyPressed(key);    
			else OnKeyReleased(key);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (ScriptReady) {
			OnProcess(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (ScriptReady) {
			OnPhysics(delta);
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		OnCreation();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		OnDeletion();
	}

	#endregion

	private bool _enabled = true;

	[Export]
	public bool Enabled
	{
		get => _enabled;
		set
		{
			if (value)
			{
				ProcessMode = ProcessModeEnum.Inherit;   
			}
			else
			{
				ProcessMode = ProcessModeEnum.Disabled;
			}
			_enabled = value;
		}
	}

	#region methods


	/// <summary>
	/// Checks if the script is running on the server (STATIC)
	/// </summary>
	public static async Task<bool> isServer(object _ = null)
	{
		var repl = await Game.Instance();
		return repl.Multiplayer.IsServer();
	}


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	public bool isServer() => Multiplayer.IsServer();


	/// <summary>
	/// Checks if the script is running on the client (STATIC)
	/// </summary>
	public static async Task<bool> isClient(object _ = null)
	{
		return !await isServer();
	}

	/// <summary>
	/// Checks if the script is running on the client
	/// </summary>
	public bool isClient() => !Multiplayer.IsServer();

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public static bool isEditor() => Engine.IsEditorHint();

	/// <summary>
	/// Prints whatever's input
	/// </summary>
	public static void print(params object[] what) => GD.Print(what);

	/// <summary>
	/// Throws an error completely stopping current execution
	/// </summary>
	public static void error(params object[] what) => throw new Exception(
		string.Join(' ', [.. what.Select( a => a.ToString() )])
	);

	/// <summary>
	/// Throws an error without completely stopping current execution
	/// </summary>
	public static void softError(params object[] what) => GD.PushError(what);

	/// <summary>
	/// Produces a warning in Godot's console
	/// </summary>
	public static void warn(params object[] what) => GD.PushWarning(what);

	/// <summary>
	/// If the given thing is null, false, or invalid produce an error
	/// <para/><c>
	/// assert(node, "node isn't real");
	/// </c>
	/// </summary>
	public static void assert(object thing, params object[] what) {
		if (
			thing is null 
			|| thing is bool b && b == false 
			|| thing is Node n && !IsInstanceValid(n)    
		) error(what);
	}

	#endregion
}
