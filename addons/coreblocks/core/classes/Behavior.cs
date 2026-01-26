using System;
using System.Linq;
using System.Reflection;
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
/// <para/><c> + OnKeyPressed(InputEventKey key, bool shouldBeUnhandled) </c>
/// <para/><c> + OnKeyReleased(InputEventKey key, bool shouldBeUnhandled) </c>
/// <para/><c> + OnInput(InputEvent @event, bool shouldBeUnhandled) </c>
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

[Tool]
[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class Behavior : Node
{
	public bool ScriptReady = false;

	#region overridable events

	/// <summary>
	/// checks if an action is actively being pressed
	/// </summary>
	/// <param name="actionName">name of the action to check for</param>
	/// <param name="shouldBeUnhandled">if the user is unfocused and the input is untouched by anything else</param>
	public bool isActionPressed(StringName actionName, bool shouldBeUnhandled = false) => 
		shouldBeUnhandled ? this.IsUnhandled() && Input.IsActionPressed(actionName) : Input.IsActionPressed(actionName);


	/// <summary>
	/// checks if an action was just released
	/// </summary>
	/// <param name="actionName">name of the action to check for</param>
	/// <param name="shouldBeUnhandled">if the user is unfocused and the input is untouched by anything else</param>
	public bool wasActionJustReleased(StringName actionName, bool shouldBeUnhandled  = false) => 
		shouldBeUnhandled ? this.IsUnhandled() && Input.IsActionJustReleased(actionName) : Input.IsActionJustReleased(actionName);

	
	/// <summary>
	/// checks if an action was just pressed
	/// </summary>
	/// <param name="actionName">name of the action to check for</param>
	/// <param name="shouldBeUnhandled">if the user is unfocused and the input is untouched by anything else</param>
	public bool wasActionJustPressed(StringName actionName, bool shouldBeUnhandled  = false) => 
		shouldBeUnhandled ? this.IsUnhandled() && Input.IsActionJustPressed(actionName) : Input.IsActionJustPressed(actionName);

	/// <summary>
	/// checks if a key is actively being pressed
	/// </summary>
	/// <param name="key">key to check for</param>
	/// <param name="shouldBeUnhandled">if the user is unfocused and the input is untouched by anything else</param>
	public bool isKeyPressed(Key key, bool shouldBeUnhandled  = false) => 
		shouldBeUnhandled ? this.IsUnhandled() && Input.IsKeyPressed(key) : Input.IsKeyPressed(key);


	/// <summary>
	/// Overridable event function for when a key is pressed
	/// </summary>
	/// <param name="key">key event</param>
	/// <param name="unhandled">if the user is unfocused and the input is untouched by anything else</param>
	public virtual void OnKeyPressed(InputEventKey key, bool unhandled)
	{
	}

	/// <summary>
	/// Overridable event function for when a key was just released
	/// </summary>
	/// <param name="key">key event</param>
	/// <param name="unhandled">if the user is unfocused and the input is untouched by anything else</param>
	public virtual void OnKeyReleased(InputEventKey key, bool unhandled)
	{
	}


	/// <summary>
	/// Overridable event function for when an input was just made
	/// </summary>
	/// <param name="event">input event</param>
	/// <param name="unhandled">if the user is unfocused and the input is untouched by anything else</param>
	public virtual void OnInput(InputEvent @event, bool unhandled)
	{
	}


	/// <summary>
	/// Overridable event function for when the user tabs into the window
	/// </summary>
	public virtual void OnTabbedIn()
	{
	}


	/// <summary>
	/// Overridable event function for when the user tabs out of the window
	/// </summary>
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

	#endregion

	#region networking methods

	/// <summary>
	/// if the behavior is a networker meaning it doesn't wait for the game to be truly ready
	/// </summary>
	public bool IsPrerunner()
		=> Attribute.IsDefined(GetType(), typeof(PrerunnerAttribute));

	/// <summary>
	/// the current run context of the script, optional type argument for functions to check what context a specific method is in
	/// </summary>
	/// <param name="methodName">optional method to check the context of</param>
	/// <returns></returns>
	#nullable enable
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public Enum.RunContext GetContext(string? methodName = null) {
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
			? Enum.RunContext.Server 
			: Enum.RunContext.Client;
	}


	/// <summary>
	/// boolean check if the following code should be able to run in the current context
	/// </summary>
	/// <param name="methodName">optional method to check the context of</param>
	/// <returns></returns>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool CanRunOnContext(string? methodName = null)
	{
		if (!Game.IsConnected()) return false;

		var context = GetContext(methodName);

		return (context == Enum.RunContext.Server && isServer()) || context == Enum.RunContext.Client;
	}

	#nullable disable
	#endregion

	#region ready

	public override async void _Ready()
	{
		if (Enabled)
			ProcessMode = ProcessModeEnum.Inherit;   
		else
			ProcessMode = ProcessModeEnum.Disabled;

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

		if (!IsPrerunner())
			await Game.WaitUntilConnected();

		if (IsPrerunner() || CanRunOnContext("OnReady")) 
			OnReady();
		
		ScriptReady = true;
	}

	#endregion

	#region event handlers

	public override async void _Notification(int what)
	{
		base._Notification(what);

		if (!Game.IsConnected()) return;

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

	public override async void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!Game.IsConnected()) return;

		var guh = this.IsUnhandled();

		if (CanRunOnContext("OnInput"))
			OnInput(@event, guh);

		if (@event is InputEventKey key)
		{
			if (key.Pressed && CanRunOnContext("OnKeyPressed"))
				OnKeyPressed(key, guh);

			else if (CanRunOnContext("OnKeyReleased"))
				OnKeyReleased(key, guh);
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!Game.IsConnected()) return;

		if (ScriptReady && CanRunOnContext("OnProcess")) {
			OnProcess(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!Game.IsConnected()) return;

		if (ScriptReady && CanRunOnContext("OnPhysics")) {
			OnPhysics(delta);
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();

		if (!Game.IsConnected()) return;

		if (CanRunOnContext("OnCreation"))
		{
			OnCreation();	
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (!Game.IsConnected()) return;

		if (CanRunOnContext("OnDeletion"))
		{
			OnDeletion();	
		}
	}

	#endregion

	#region exports

	private bool enabled = true;

	[Export]
	public bool Enabled
	{
		get => Engine.IsEditorHint() ? enabled : ProcessMode != ProcessModeEnum.Disabled;
		set
		{
			if (Engine.IsEditorHint()) 
				enabled = value;
			else
				ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;	
		}
	}

	#endregion

	#region methods


	/// <summary>
	/// Checks if the script is running on the server (STATIC)
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool isServer(object _ = null)
		=> Game.IsServer();


	/// <summary>
	/// Checks if the script is running on the server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool isServer() 
		=> Game.IsServer();


	/// <summary>
	/// Checks if the script is running on the client (STATIC)
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public static bool isClient(object _ = null)
		=> !isServer();

	/// <summary>
	/// Checks if the script is running on the client
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool isClient() 
		=> !isServer();

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public static bool isEditor() 
		=> Game.IsEditor();

	

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
