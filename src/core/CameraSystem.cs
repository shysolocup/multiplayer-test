using Godot;
using PhantomCamera;
using PhantomCamera.Manager;

/// <summary>
/// controls cameras
/// </summary>
[GlobalClass, Icon("uid://nxhbjfaqej4p")]
public partial class CameraSystem : Singleton3D<CameraSystem>
{

	public PhantomCamera3D MainCamera { get; set; }
	public Camera3D Camera { get; set; }

	public PhantomCameraHost Host { get; set; }

	public override void _Ready()
	{
		base._Ready();

		Camera = GetNode<Camera3D>("./camera");
		MainCamera = GetNode<Node3D>("./main").AsPhantomCamera3D();
		
		Host = PhantomCameraManager.PhantomCameraHosts[0];
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
