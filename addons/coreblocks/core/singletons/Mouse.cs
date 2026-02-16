using Godot;
using Godot.Collections;
using System.Collections.Generic;

[NotReplicated]
[GlobalClass, Icon("uid://bd2ep30fvfeix")]
public partial class Mouse : Singleton<Mouse>
{

	/// <summary>
	/// an array of <see cref="ActorBinding"/>s with actors that control what should get priority over the mouse's lock
	/// </summary>
	[Export]
	public Godot.Collections.Dictionary<int, ActorBinding<Input.MouseModeEnum>> PriorityList = [];


	/// <summary>
	/// binds a <see cref="ActorBinding"/> which controls what should get priority over the mouse's lock
	/// 
	/// <para/> if the mode is set to <see cref="Input.MouseModeEnum.Captured"/> the mouse will be invisible and locked into the window
	/// <para/> if the mode is set to <see cref="Input.MouseModeEnum.Visible"/> the mouse will be unlocked and visible
	/// </summary>
	public ActorBinding<Input.MouseModeEnum> BindActor(Node actor, Enum.PriorityChannel channel, bool persist = false, Input.MouseModeEnum? mode = null)
		=> BindActor(actor, (int)channel, persist, mode);


	/// <summary>
	/// binds a <see cref="ActorBinding"/> to the given priority channel which controls what should get priority over the mouse's mode
	/// <para/>if the persist param is false (default) it'll set the mode to null once all actors are disconnected from the channel
	/// </summary>
	/// <param name="actor">acting node that </param>
	/// <param name="channel">priority level, optionally an int</param>
	/// <param name="persist">if the mode should persist after all actors are unbound, default false meaning once all the actors disconnect it'll set the mode to null</param>
	/// <returns></returns>
	public ActorBinding<Input.MouseModeEnum> BindActor(Node actor, int channel, bool persist = false, Input.MouseModeEnum? mode = null)
	{
		if (PriorityList.GetValueOrDefault(channel) is ActorBinding<Input.MouseModeEnum> value)
		{
			if (!value.Actors.Contains(actor))
				value.Actors.Add(actor);

			value.Persist = persist;

			if (mode is not null)
				value.Value = mode;
		}
		else
		{
			var binding = new ActorBinding<Input.MouseModeEnum>
			{
				Actors = [actor],
				Persist = persist,
			};

			if (mode is not null)
				binding.Value = mode;

			PriorityList[channel] = binding;
				
		}

		return PriorityList.GetValueOrDefault(channel);
	}


	public ActorBinding<Input.MouseModeEnum> GetBindng(Enum.PriorityChannel channel)
		=> PriorityList.GetValueOrDefault((int)channel);

	public ActorBinding<Input.MouseModeEnum> GetBindng(int channel)
		=> PriorityList.GetValueOrDefault(channel);

	public Array<Node> GetActors(Enum.PriorityChannel channel)
		=> GetBindng(channel).Actors;

	public Array<Node> GetActors(int channel)
		=> GetBindng(channel).Actors;


	/// <summary>
	/// sets the mode of the <see cref="ActorBinding"/>
	/// </summary>
	public Mouse SetBindingMode(Enum.PriorityChannel channel, Input.MouseModeEnum mode)
		=> SetBindingMode((int)channel, mode);


	/// <summary>
	/// sets the mode of the <see cref="ActorBinding"/>
	/// </summary>
	public Mouse SetBindingMode(int channel, Input.MouseModeEnum mode)
	{
		if (PriorityList.GetValueOrDefault(channel) is ActorBinding<Input.MouseModeEnum> value)
		{
			value.Value = mode;
		}
		return this;
	}

	/// <summary>
	/// sets the mode of the <see cref="ActorBinding"/>
	/// </summary>
	public Mouse SetBindingPersist(int channel, bool persist)
	{
		if (PriorityList.GetValueOrDefault(channel) is ActorBinding<Input.MouseModeEnum> value)
		{
			value.Persist = persist;
		}
		return this;
	}

	/// <summary>
	/// unbinds a <see cref="ActorBinding"/>
	/// </summary>
	public bool UnbindActor(Node actor, Enum.PriorityChannel channel)
		=> UnbindActor(actor, (int)channel);

	/// <summary>
	/// unbinds a <see cref="ActorBinding"/>
	/// </summary>
	public bool UnbindActor(Node actor, int channel)
	{
		if (PriorityList.GetValueOrDefault(channel) is ActorBinding<Input.MouseModeEnum> value)
		{
			return value.Actors.Remove(actor);
		}
		return false;
	}


	public override void _Process(double delta)
	{
		base._Process(delta);

		int? nullablePriority = null;

		foreach ((var i, var binding) in PriorityList)
		{
			// if htehe actors are greater than 0 and it has the stuff and it works then it does
			if (
				binding.Acted
				&& (nullablePriority is null || i < nullablePriority) 
				&& binding.Value is Input.MouseModeEnum
			) 
				nullablePriority = i;

			// iff ffff all actors stop acting on the bind then reset its mode back to null
			else if (binding.Acted && !binding.Persist)
				binding.Value = null;
		}

		// oouegh if nullable priority exists thn get the prioirity and get the bind mode and apply it
		if (
			nullablePriority is int priority 
			&& PriorityList.TryGetValue(priority, out var bind)
			&& bind.Value is Input.MouseModeEnum mode
		)
			Input.MouseMode = mode;
	}

	[Export]
	public RayCast3D Ray { get; set; }

	public Vector2 Position
	{
		get => GetViewport().GetMousePosition();
	}

	#nullable enable

	public T? GetTarget<T>(int range = 1000) where T : Node3D
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
	public override async void _Ready()
	{
		var game = await Game.Instance();
		
		base._Ready();
		Ray ??= new RayCast3D();
		AddChild(Ray);

		if (Engine.IsEditorHint()) 
			Ray.Owner = game;
	}
}
