using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// controls cameras
/// </summary>
[GlobalClass, Icon("uid://nxhbjfaqej4p")]
public partial class CameraSystem : Singleton3D<CameraSystem>
{

	[Export] public float Sensitivity = 0.003f;
	[Export] public float TargetZoom = 10;
	[Export] public float MaxZoom = 15;
	[Export] public float MinZoom = 1;
	[Export] public float ZoomStep = 1;
	[Export] public float ShiftLockOffset = 0.5f;
	[Export] public bool ShiftLocked = false;
	[Export] public Vector3 CameraOffset = Vector3.Zero;

	public Camera3D CurrentCamera { 
		get => GetViewport().GetCamera3D(); 
		set => value.MakeCurrent(); 
	}

	public Camera3D ThirdPersonCamera { get; set; }
	public SpringArm3D ThirdPersonSpring { get; set; }

	private static Character _target { get; set; }

	[Export] public Character Target { 
		get => _target; 
		set
		{
			_target = value;
			GD.Print($"set camera target to {value}");
		} 
	}

	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true
		)
	]
	private void _setTarget(Player player)
	{
		if (Multiplayer.GetUniqueId() == Multiplayer.GetRemoteSenderId())
		{
			Target = player.GetCharacter();	
		}
	}

	/// <summary>
	/// Sets the local player of the client using the player's built in id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	
	public Error SetTarget(Player player)
	{
		return RpcId(player.GetPeerId(), MethodName._setTarget, player);
	}

	public override void _Ready()
	{
		base._Ready();

		ThirdPersonSpring = GetNode<SpringArm3D>("./spring");
		ThirdPersonCamera = GetNode<Camera3D>("./spring/thirdPerson");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		Input.MouseMode = (Input.IsActionPressed("camera_move") || ShiftLocked) 
			? Input.MouseModeEnum.Captured 
			: Input.MouseModeEnum.Visible;


		if (Target is not null && Target.Head is not null)
		{
			var pos = ThirdPersonSpring.GlobalPosition;
			var newPos = Target.Head.GlobalPosition;

			var basis = ThirdPersonSpring.GlobalBasis;

			var forward = basis.Z;
			var right = basis.X;
			var up = basis.Y;

			newPos -= forward * CameraOffset.Z;
			newPos += right * CameraOffset.X;
			newPos += up * CameraOffset.Y;

			if (ShiftLocked)
			{
				newPos += right * ShiftLockOffset;
				newPos += up * (ShiftLockOffset/2);
			}

			ThirdPersonSpring.GlobalPosition = pos.Lerp(newPos, 50*(float)delta);
		}

		ThirdPersonSpring.SpringLength = Mathf.Lerp(ThirdPersonSpring.SpringLength, TargetZoom, 20 * (float)delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (Input.IsActionJustPressed("shift_lock"))
		{
			ShiftLocked ^= true;
		}

		if (@event is InputEventMouseButton wheel)
		{
			if (wheel.ButtonIndex == MouseButton.WheelUp)
			{
				TargetZoom = Mathf.Clamp(TargetZoom - ZoomStep, MinZoom, MaxZoom);
			}

			else if (wheel.ButtonIndex == MouseButton.WheelDown)
			{
				TargetZoom = Mathf.Clamp(TargetZoom + ZoomStep, MinZoom, MaxZoom);
			}
		}
	}


	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (
			@event is InputEventMouseMotion mouse && 
			(Input.IsActionPressed("camera_move") || ShiftLocked) && // if shiftlocked or holding rmb move camera to mouse relative
			Target is not null && 
			ThirdPersonCamera.Current
		)
		{
			var rot = ThirdPersonSpring.Rotation;

			var realSens = Sensitivity;

			rot.Y -= mouse.Relative.X * realSens;
			rot.Y = Mathf.Wrap(rot.Y, 0, float.Tau);

			rot.X -= mouse.Relative.Y * realSens;
			rot.X = Mathf.Clamp(rot.X, -float.Pi/2, float.Pi/4);

			ThirdPersonSpring.Rotation = rot;
		}
	}
}
