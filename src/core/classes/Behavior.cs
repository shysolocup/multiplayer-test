using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Godot;

#region hellish summary
/// <summary>
/// a physical script node with useful global dependencies and events
/// 
/// <para/> Global variables and dependencies:
/// 
/// <list type="bullet">
/// <item>
/// 
/// <description>
/// <b>game:</b>
/// <c><see cref="Game"/></c> 
/// root game
/// </description>
///
/// </item><item>
/// 
/// <description>
/// <b>workspace:</b> 
/// <c><see cref="Workspace"/></c>
/// place for things in 3D space
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>cameras:</b> 
/// <c><see cref="CameraSystem"/></c>
/// camera storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>lighting:</b> 
/// <c><see cref="LightingSystem"/></c>
/// lighting controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>maps:</b> 
/// <c><see cref="MapSystem"/></c>
/// map controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>characters:</b> 
/// <c><see cref="Characters"/></c>
/// character model storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>players:</b> 
/// <c><see cref="Players"/></c>
/// player storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>gui:</b> 
/// <c><see cref="GuiSystem"/></c>
/// ui storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>audios:</b> 
/// <c><see cref="AudioSystem"/></c>
/// audio storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>shaders:</b> 
/// <c><see cref="ShaderSystem"/></c>
/// shader storage and controller
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>global:</b> 
/// <c><see cref="GlobalStorage"/></c>
/// global replicated storage
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>client:</b> 
/// <c><see cref="Client"/></c>
/// client singleton 
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>server:</b> 
/// <c><see cref="Server"/></c>
/// server singleton 
/// </description>
/// 
/// </item><item>
/// 
/// <description>
/// <b>rep:</b> 
/// <c><see cref="Replicator"/></c>
/// replicator singleton 
/// </description>
/// 
/// </item></list>
/// 
/// <para/> Basic event functions:
/// 
/// <para/><c> + OnReady() </c> when the script node and its dependencies are ready
/// <para/><c> + OnProcess(double delta) </c> ran every frame, delta is the time between frames
/// <para/><c> + OnPhysics(double delta) </c> ran every physics simulation, delta is the time between frames
/// 
/// <para/> Context event functions:
/// 
/// <para/><c> + OnJoin() </c> ran when the client joins
/// <para/><c> + OnLeave </c> ran when the client leaves
/// 
/// <para/> Input event functions:
/// 
/// <para/><c> + OnKeyPressed(InputEventKey key) </c>
/// <para/><c> + OnKeyReleased(InputEventKey key) </c>
/// <para/><c> + OnInput(InputEvent @event) </c>
/// 
/// <para/> Tab event functions:
/// 
/// <para/><c> + OnTabbedIn() </c> when the user tabs back into of the window
/// <para/><c> + OnTabbedOut() </c> when the user tabs out of the window
/// 
/// <para/> Init event functions:
/// 
/// <para/><c> + OnCreation() </c> when the script is created/reloaded
/// <para/><c> + OnDeletion() </c> whne the script is deleted/reloaded
/// 
/// </summary>
#endregion

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

	public virtual void OnInput(InputEvent @event)
	{
	}

	public virtual void OnTabbedIn()
	{
	}

	public virtual void OnTabbedOut()
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


	/// <summary>
	/// global script variable for accessing the replicator/lobby controls
	/// </summary>
	public static Replicator rep;
	/// <summary>
	/// global script variable for accessing the client
	/// </summary>
	public static Client client;

	/// <summary>
	/// global script variable for accessing the server
	/// </summary>
	public static Server server;

	/// <summary>
	/// global script variable for accessing the audios inside the workspace
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static AudioSystem audios;

	/// <summary>
	/// global script variable for accessing the cameras controller inside the workspace
	/// <para/><code>
	/// game
	///	└── client
	///	    └── cameras
	/// </code>
	/// </summary>
	public static CameraSystem cameras;

	/// <summary>
	/// global script variable for accessing the characters inside the workspace
	/// <para/><code>
	/// game
	///	└── workspace
	///	    └── characters
	/// </code>
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static Characters characters;

	/// <summary>
	/// global script variable for accessing the game
	/// <para/>the root singleton
	/// </summary>
	public static Game game;

	/// <summary>
	/// global script variable for accessing the global storage
	/// <para/><code>
	/// game
	///	└── global
	/// </code>
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static GlobalStorage global;

	/// <summary>
	/// global script variable for accessing the lighting inside the workspace
	/// <para/><code>
	/// game
	///	└── workspace
	///	    └── lighting
	/// </code>
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static LightingSystem lighting;

	/// <summary>
	/// global script variable for accessing the map controller inside the workspace
	/// <para/><code>
	/// game
	///	└── workspace
	///	    └── map
	/// </code>
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static MapSystem map;

	/// <summary>
	/// global script variable for accessing the players
	/// <para/><code>
	/// game
	///	└── players
	/// </code>
	/// <para/><b>REPLICATED:</b> nodes inside players are automatically synced
	/// </summary>
	public static Players players;

	/// <summary>
	/// global script variable for accessing the workspace
	/// <para/>workspace is the main place for anything 3D including character models, maps, lighting, cameras, and spatial audio
	/// <para/><code>
	/// game
	///	└── workspace
	///	    ├── characters
	///	    ├── map
	///	    ├── lighting
	///	    ├── cameras
	///	    └── audios
	/// </code>
	/// </summary>
	public static Workspace workspace;

	/// <summary>
	/// global script variable for accessing the client's gui
	/// <para/>this is where ui is handled
	/// <para/><b>NOTE:</b> shaders are contained within the gui since they're technically ui
	/// <para/><code>
	/// game
	///	└── client
	/// 	└── gui
	///	    	└── shaders
	/// </code>
	/// <para/>@client
	/// </summary>
	public static GuiSystem gui;

	/// <summary>
	/// global script variable for accessing the client's shaders
	/// <para/>this is where canvas shaders are implemented
	/// <para/><b>NOTE:</b> shaders are technically ui so it's stored in <see cref="GuiSystem"/>
	/// <para/><code>
	/// game
	///	└── client
	/// 	└── gui
	///	    	└── shaders
	/// </code>
	/// <para/>@client
	/// </summary>
	public static ShaderSystem shaders;
	
	/// <summary>
	/// global script variable for accessing the client's mouse
	/// <para/><code>
	/// game
	///	└── client
	///	    └── mouse
	/// </code>
	/// <para/>@client
	/// </summary>
	public static Mouse mouse;

	/// <summary>
	/// library of functions for threading and tasks
	/// 
	/// </summary>
	public static TaskLib task;

	/// <summary>
	/// global script variable for easy use of the <c>.json</c> and <c>.jsonc</c> file extensions
	/// </summary>
	public static JsonLib json;

	/// <summary>
	/// global script variable for accessing and modifying local files
	/// </summary>
	public static FileLib files;

	/// <summary>
	/// the current run context of the script, optional type argument for functions to check what context a specific method is in
	/// </summary>
	/// <param name="methodName">optional method to check the context of</param>
	/// <returns></returns>
	#nullable enable
	public ContextEnum GetContext(string? methodName = null) {
		var t = GetType();
		
		var onServer = typeof(OnServerAttribute);
		var onClient = typeof(OnClientAttribute);

		// scripst by default run on the client unless told otherwise
		// if the script is told to run on the server it should default the method to the server unless told otherwise
		bool scriptIsServer = Attribute.IsDefined(t, onServer);

		MethodInfo? method = methodName is not null ? t.GetMethod(methodName) : null;

		// if the method is told to run on the server it'll override the client
		bool methodIsServer = method is not null && Attribute.IsDefined(method, onServer);

		// if the method is explicitly told to run on the client then it'll ovrderride the overrdieedfjkld
		bool methodIsClient = method is not null && Attribute.IsDefined(method, onClient);

		return (scriptIsServer || methodIsServer) && !methodIsClient
			? ContextEnum.Server 
			: ContextEnum.Client;
	}


	/// <summary>
	/// boolean check if the following code should be able to run in the current context
	/// </summary>
	/// <param name="methodName">optional method to check the context of</param>
	/// <returns></returns>
	public bool CanRunOnContext(string? methodName = null)
	{
		if (!Replicator.IsConnected()) return false;

		var context = GetContext(methodName);
		return context == ContextEnum.Server && isServer() || context == ContextEnum.Client;
	}

	#nullable disable

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

		rep ??= await Replicator.Instance();
		client ??= await Client.Instance();
		server ??= await Server.Instance();
		audios ??= await AudioSystem.Instance();
		cameras ??= await CameraSystem.Instance();
		characters ??= await Characters.Instance();
		game ??= await Game.Instance();
		global ??= await GlobalStorage.Instance();
		lighting ??= await LightingSystem.Instance();
		map ??= await MapSystem.Instance();
		players ??= await Players.Instance();
		workspace ??= await Workspace.Instance();
		gui ??= await GuiSystem.Instance();
		shaders ??= await ShaderSystem.Instance();
		mouse ??= await Mouse.Instance();

		task ??= await TaskLib.Instance();
		json ??= await JsonLib.Instance();
		files ??= await FileLib.Instance();

		await Replicator.WaitUntilConnected();

		if (CanRunOnContext("OnReady")) 
			OnReady();
		
		ScriptReady = true;
	}

	#endregion

	#region event handlers

	public override async void _Notification(int what)
	{
		base._Notification(what);

		if (!Replicator.IsConnected()) return;

		if (what == NotificationApplicationFocusIn)
		{
			if (CanRunOnContext("OnTabbedIn"))
				OnTabbedIn();
		}
		else if (what == NotificationApplicationFocusOut)
		{
			if (CanRunOnContext("OnTabbedOut"))
				OnTabbedOut();
		}
	}

	public override async void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (!Replicator.IsConnected()) return;

		if (CanRunOnContext("OnInput"))
			OnInput(@event);

		if (@event is InputEventKey key)
		{
			if (key.Pressed && CanRunOnContext("OnKeyPressed")) OnKeyPressed(key);    
			else if (CanRunOnContext("OnKeyReleased")) OnKeyReleased(key);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!Replicator.IsConnected()) return;

		if (ScriptReady && CanRunOnContext("OnProcess")) {
			OnProcess(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!Replicator.IsConnected()) return;

		if (ScriptReady && CanRunOnContext("OnPhysics")) {
			OnPhysics(delta);
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();

		if (!Replicator.IsConnected()) return;

		if (CanRunOnContext("OnCreation"))
		{
			OnCreation();	
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (!Replicator.IsConnected()) return;

		if (CanRunOnContext("OnDeletion"))
		{
			OnDeletion();	
		}
	}

	#endregion

	#region exports

	public enum ContextEnum
	{	
		Client = 0,
		Server = 1
	}

	[Export]
	public bool Enabled
	{
		get => ProcessMode != ProcessModeEnum.Disabled;
		set => ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
	}

	#endregion

	#region methods


	/// <summary>
	/// Checks if the script is running on the server (STATIC)
	/// </summary>
	public static bool isServer(object _ = null)
		=> Replicator.IsServer();


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	public bool isServer() 
		=> Replicator.IsServer();


	/// <summary>
	/// Checks if the script is running on the client (STATIC)
	/// </summary>
	public static bool isClient(object _ = null)
		=> !isServer();

	/// <summary>
	/// Checks if the script is running on the client
	/// </summary>
	public bool isClient() 
		=> !isServer();

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public static bool isEditor() 
		=> Engine.IsEditorHint();

	/// <summary>
	/// Prints whatever's input
	/// </summary>
	public static void print(params object[] what) 
	=> GD.Print(what);

	/// <summary>
	/// Throws an error completely stopping current execution
	/// </summary>
	public static void error(params object[] what) 
		=> throw new Exception(
			string.Join(' ', [.. what.Select( a => a.ToString() )])
		);

	/// <summary>
	/// Throws an error without completely stopping current execution
	/// </summary>
	public static void softError(params object[] what) 
		=> GD.PushError(what);

	/// <summary>
	/// Produces a warning in Godot's console
	/// </summary>
	public static void warn(params object[] what) 
		=> GD.PushWarning(what);

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

	/// <summary>
	/// If the given thing is null, false, or invalid produce an error
	/// <para/><c>
	/// assert(node, "node isn't real");
	/// </c>
	/// </summary>
	public static void softAssert(object thing, params object[] what) {
		if (
			thing is null 
			|| thing is bool b && b == false 
			|| thing is Node n && !IsInstanceValid(n)    
		) softError(what);
	}

	#endregion
}
