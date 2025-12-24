using System;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Godot;
using Godot.Bridge;

[Tool]
[GlobalClass, Icon("uid://dme3m2uv5jaju")]
public partial class ScriptBase : Node
{

    public bool ScriptReady = false;

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

    public override void _Ready()
    {
        base._Ready();

        OnReady();
        ScriptReady = true;
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
		OnStart();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
		OnEnd();
    }

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
}