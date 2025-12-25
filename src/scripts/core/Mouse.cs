using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass, Icon("")]
public partial class Mouse : Singleton<Mouse>
{
	private RayCast3D Ray { get; set; }

	public Vector2 Position
	{
		get => GetViewport().GetMousePosition();
	}


	public async Task<T> GetTarget<T>(int range = 1000) where T : Node3D
	{
		var origin = Client.Camera.ProjectRayOrigin(Position);
		var end = Client.Camera.ProjectRayNormal(Position) * range;

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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Ray = new RayCast3D();
		AddChild(Ray);
	}
}
