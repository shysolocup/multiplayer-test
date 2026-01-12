using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class MouseModeBinding : Resource
{
	public Array<Node> Actors = [];
	public Input.MouseModeEnum? Mode;
	public bool Persist = false;
}

[GlobalClass, Icon("uid://bd2ep30fvfeix")]
public partial class Mouse : Singleton<Mouse>
{

	/// <summary>
	/// an array of <see cref="MouseModeBinding"/>s with actors that control what should get priority over the mouse's mode
	/// </summary>
	[Export]
	public Godot.Collections.Dictionary<int, MouseModeBinding> PriorityList = [];


	/// <summary>
	/// binds a <see cref="MouseModeBinding"/> which controls what should get priority over the mouse's mode
	/// </summary>
	public MouseModeBinding BindActor(Node actor, PriorityChannel priority, bool persist = false, Input.MouseModeEnum? mode = null)
		=> BindActor(actor, (int)priority, persist, mode);


	/// <summary>
	/// binds a <see cref="MouseModeBinding"/> to the given priority channel which controls what should get priority over the mouse's mode
	/// <para/>if the persist param is false (default) it'll set the mode to null once all actors are disconnected from the channel
	/// </summary>
	/// <param name="actor">acting node that </param>
	/// <param name="priority">priority level, optionally an int</param>
	/// <param name="persist">if the mode should persist after all actors are unbound, default false meaning once all the actors disconnect it'll set the mode to null</param>
	/// <returns></returns>
	public MouseModeBinding BindActor(Node actor, int priority, bool persist = false, Input.MouseModeEnum? mode = null)
	{
		if (PriorityList.GetValueOrDefault(priority) is MouseModeBinding value)
		{
			if (!value.Actors.Contains(actor))
				value.Actors.Add(actor);

			value.Persist = persist;

			if (mode is not null)
				value.Mode = mode;
		}
		else
		{
			var binding = new MouseModeBinding
			{
				Actors = [actor],
				Persist = persist,
			};

			if (mode is not null)
				binding.Mode = mode;

			PriorityList[priority] = binding;
				
		}

		return PriorityList.GetValueOrDefault(priority);
	}


	public MouseModeBinding GetBindng(PriorityChannel priority)
		=> PriorityList.GetValueOrDefault((int)priority);

	public MouseModeBinding GetBindng(int priority)
		=> PriorityList.GetValueOrDefault(priority);

	public Array<Node> GetActors(PriorityChannel priority)
		=> GetBindng(priority).Actors;

	public Array<Node> GetActors(int priority)
		=> GetBindng(priority).Actors;


	/// <summary>
	/// sets the mode of the <see cref="MouseModeBinding"/>
	/// </summary>
	public Mouse SetBindingMode(PriorityChannel priority, Input.MouseModeEnum mode)
		=> SetBindingMode((int)priority, mode);


	/// <summary>
	/// sets the mode of the <see cref="MouseModeBinding"/>
	/// </summary>
	public Mouse SetBindingMode(int priority, Input.MouseModeEnum mode)
	{
		if (PriorityList.GetValueOrDefault(priority) is MouseModeBinding value)
		{
			value.Mode = mode;
		}
		return this;
	}

	/// <summary>
	/// sets the mode of the <see cref="MouseModeBinding"/>
	/// </summary>
	public Mouse SetBindingPersist(int priority, bool persist)
	{
		if (PriorityList.GetValueOrDefault(priority) is MouseModeBinding value)
		{
			value.Persist = persist;
		}
		return this;
	}

	/// <summary>
	/// unbinds a <see cref="MouseModeBinding"/>
	/// </summary>
	public bool UnbindActor(Node actor, PriorityChannel priority)
		=> UnbindActor(actor, (int)priority);

	/// <summary>
	/// unbinds a <see cref="MouseModeBinding"/>
	/// </summary>
	public bool UnbindActor(Node actor, int priority)
	{
		if (PriorityList.GetValueOrDefault(priority) is MouseModeBinding value)
		{
			return value.Actors.Remove(actor);
		}
		return false;
	}


	public enum PriorityChannel
	{
		MasterHigh,
		MasterMedium,
		MasterLow,
		Ui,
		Camera
	}


	public override void _Process(double delta)
	{
		base._Process(delta);

		int? nullablePriority = null;

		foreach ((var i, var binding) in PriorityList)
		{
			if (
				binding.Actors.Count > 0 
				&& (nullablePriority is null || i < nullablePriority) 
				&& binding.Mode is Input.MouseModeEnum
			) {
				nullablePriority = i;
			}
			else if (binding.Actors.Count > 0 && !binding.Persist)
			{
				binding.Mode = null;
			}
		}

		if (
			nullablePriority is int priority 
			&& PriorityList.TryGetValue(priority, out var bind)
			&& bind.Mode is Input.MouseModeEnum mode
		)
		{
			Input.MouseMode = mode;
		}
	}

	private RayCast3D Ray { get; set; }

	public Vector2 Position
	{
		get => GetViewport().GetMousePosition();
	}

	#nullable enable

	public async Task<T?> GetTarget<T>(int range = 1000) where T : Node3D
	{
		var camera = Client.CurrentCamera;
		
		if (camera is null) return null;

		var origin = camera.ProjectRayOrigin(Position);
		var end = camera.ProjectRayNormal(Position) * range;

		var transform = Ray.GlobalTransform;
		transform.Origin = origin;
		Ray.GlobalTransform = transform;

		Ray.ForceRaycastUpdate();

		if (Ray.IsColliding() && Ray.GetCollider() is T obj)
		{
			return obj;
		}

		return null;
	}

	#nullable disable

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Ray = new RayCast3D();
		AddChild(Ray);
	}
}
