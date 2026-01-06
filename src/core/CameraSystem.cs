using Godot;
using PhantomCamera;

/// <summary>
/// controls cameras
/// </summary>
[GlobalClass, Icon("uid://nxhbjfaqej4p")]
public partial class CameraSystem : Singleton3D<CameraSystem>
{

	public PhantomCamera3D MainCamera { get; set; }
	public Camera3D Camera { get; set; }

	public override void _Ready()
	{
		base._Ready();

		MainCamera = GetNode<PhantomCamera3D>("./main");
		Camera = GetNode<Camera3D>("./camera");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
