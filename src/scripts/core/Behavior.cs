using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;


[Tool]
[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class Behavior : Node
{
    public bool ScriptReady = false;

    #region overridable events

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public virtual void OnServer()
    {   
    }

    public virtual void OnClient()
    {
    }

	public virtual void OnStart()
	{
	}

    public virtual void OnEnd()
	{
	}

	public virtual void OnReady()
	{
	}

	public virtual void OnProcess(double delta)
	{
	}

	public virtual void OnPhysics(double delta)
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

		// it's specifically just in this order so ready fires before processes
		// this might lead to issues but we ball
		OnReady();
        if (IsServer())
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
		OnStart();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
		OnEnd();
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
    public static async Task<bool> IsServer(object _ = null)
    {
        var repl = await Game.Instance();
        return repl.Multiplayer.IsServer();
    }

    /// <summary>
    /// Checks if the script is running on the server
    /// </summary>
    public bool IsServer() => Multiplayer.IsServer();


    /// <summary>
    /// Checks if the script is running on the client (STATIC)
    /// </summary>
    public static async Task<bool> IsClient(object _ = null)
    {
        return !await IsServer();
    }

    /// <summary>
    /// Checks if the script is running on the client
    /// </summary>
    public bool IsClient() => !Multiplayer.IsServer();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsEditor() => Engine.IsEditorHint();

    public static void Print(params object[] what) => GD.Print(what);

    public static void Throw(params object[] what) => throw new Exception(
        string.Join(' ', [.. what.Select( a => a.ToString() )])
    );

    public static void Warn(params object[] what) => GD.PushWarning(what);

    /// <summary>
    /// If the given thing is 
    /// </summary>
    public static void Assert(object thing, params object[] what) {
        if (
            thing is null 
            || thing is bool b && b == false 
            || thing is Node n && !IsInstanceValid(n)    
        ) Throw(what);
    }

    #endregion
}