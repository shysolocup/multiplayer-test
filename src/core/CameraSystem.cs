using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// controls cameras
/// </summary>
[Tool]
[GlobalClass, Icon("uid://nxhbjfaqej4p")]
public partial class CameraSystem : Singleton3D<CameraSystem>
{

	#region vars

	[Signal] public delegate void CameraTypeChangedEventHandler(CameraTypeEnum cameraType);
	[Signal] public delegate void SubjectChangedEventHandler(Character subject);
	[Signal] public delegate void CurrentCameraChangedEventHandler(Camera3D camera);
	[Signal] public delegate void FreecamEnabledEventHandler();
	[Signal] public delegate void FreecamDisabledEventHandler();
	[Signal] public delegate void ShiftLockEnabledEventHandler();
	[Signal] public delegate void ShiftLockDisabledEventHandler();


	#region CameraType
	/// <summary>
	/// this is the camera type it'll go to going in and out of fullscreen
	/// this changes when the normal CameraType is changed
	/// </summary>
	public CameraTypeEnum GotoCameraType = CameraTypeEnum.ThirdPerson;

	[Export]
	public CameraTypeEnum CameraType
	{
		get {
			if (Engine.IsEditorHint())
			{
				ThirdPersonCamera ??= GetNode<Camera3D>("./thirdPerson");
				FreecamCamera ??= GetNode<Camera3D>("./freecam");
				FirstPersonCamera ??= GetNode<Camera3D>("./firstPerson");	
			}

			var value = 
				(FreecamCamera is not null && FreecamCamera.Current) || FreecamActive
					? GotoCameraType
				: ThirdPersonCamera is not null && ThirdPersonCamera.Current 
					? CameraTypeEnum.ThirdPerson
				: FirstPersonCamera is not null && FirstPersonCamera.Current
					? CameraTypeEnum.FirstPerson
				: CameraTypeEnum.Custom;

			GotoCameraType = value;

			return value;
		}

		set
		{
			if (Engine.IsEditorHint())
			{
				ThirdPersonCamera ??= GetNode<Camera3D>("./thirdPerson");
				FreecamCamera ??= GetNode<Camera3D>("./freecam");
				FirstPersonCamera ??= GetNode<Camera3D>("./firstPerson");	
			}

			GotoCameraType = value;

			CurrentCamera = 
				value == CameraTypeEnum.ThirdPerson && ThirdPersonCamera is not null
					? ThirdPersonCamera
				: value == CameraTypeEnum.FirstPerson && FirstPersonCamera is not null
					? FirstPersonCamera
				: CurrentCamera;

			EmitSignalCameraTypeChanged(value);
		}
	}

	public enum CameraTypeEnum
	{
		ThirdPerson = 0,
		FirstPerson = 1,
		Custom = 2
	}

	#endregion


	#region CurrentCamera
	/// <summary>
	/// default camera to go back to when entering freecam
	/// ignores the freecam camera and is only set to other cameras
	/// </summary>
	public Camera3D GotoCamera { get; set; }

	[Export]
	public Camera3D CurrentCamera { 
		get {
			if (Engine.IsEditorHint())
			{
				ThirdPersonCamera ??= GetNode<Camera3D>("./thirdPerson");
				FreecamCamera ??= GetNode<Camera3D>("./freecam");
				FirstPersonCamera ??= GetNode<Camera3D>("./firstPerson");	
			}

			return 
				FreecamActive || FreecamCamera is not null && FreecamCamera.Current && GotoCamera is not null
				? GotoCamera
				: GetViewport().GetCamera3D(); 
		}
		
		set {
			if (Engine.IsEditorHint())
			{
				ThirdPersonCamera ??= GetNode<Camera3D>("./thirdPerson");
				FreecamCamera ??= GetNode<Camera3D>("./freecam");
				FirstPersonCamera ??= GetNode<Camera3D>("./firstPerson");	
			}

			value ??= CameraType == CameraTypeEnum.ThirdPerson 
					? ThirdPersonCamera
				: CameraType == CameraTypeEnum.FirstPerson
					? FirstPersonCamera
				: FreecamActive && FreecamCamera is not null
					? FreecamCamera	
				: null;

			if (value != FreecamCamera)
			{
				GotoCamera = value;	

				EmitSignalCurrentCameraChanged(value);
			}

			else if (!FreecamActive)
			{
				GD.Print($"set camera to {value?.Name}");

				value.Current = true;

				EmitSignalCurrentCameraChanged(value);
			}
		}
	}
	#endregion

	[Export] public Vector3 CameraOffset = Vector3.Zero;
	[Export] public float Sensitivity = 0.003f;
	[Export] public float TargetZoom = 7;
	[Export] public float MaxZoom = 15;
	[Export] public float MinZoom = 1;
	[Export] public float ZoomStep = 1;
	[Export] public float ShiftLockOffset = 0.5f;
	[Export] public bool ShiftLocked = false;

	public Camera3D ThirdPersonCamera { get; set; }
	public Camera3D FirstPersonCamera { get; set; }
	public Camera3D FreecamCamera { get; set; }
	public SpringArm3D ThirdPersonSpring { get; set; }


	#endregion


	#region target
	private static Character _subject { get; set; }

	[Export] public Character Subject { 
		get => _subject; 
		set
		{
			_subject = value;
			GD.Print($"set camera target to {value}");

			EmitSignalSubjectChanged(value);
		} 
	}

	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true
		)
	]
	private void _setSubject(ulong id)
	{
		var node = InstanceFromId(id);

		if ( node is Player player && Multiplayer.GetUniqueId() == Multiplayer.GetRemoteSenderId())
		{
			Subject = player.GetCharacter();	
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
	
	public Error SetSubject(Player player)
	{
		return RpcId(player.GetPeerId(), MethodName._setSubject, player.GetInstanceId());
	}

	#endregion

	#region ready
	
	public override void _Ready()
	{
		base._Ready();

		ThirdPersonCamera = GetNode<Camera3D>("./thirdPerson");
		FreecamCamera = GetNode<Camera3D>("./freecam");
		FirstPersonCamera = GetNode<Camera3D>("./firstPerson");

		if (Engine.IsEditorHint()) return;

		ThirdPersonCamera.Position = Vector3.Zero;
		ThirdPersonCamera.Rotation = Vector3.Zero;

		var spring = new SpringArm3D();
		spring.Name = "thirdPersonSpring";
		AddChild(spring);

		ThirdPersonCamera.Reparent(spring);
		ThirdPersonCamera.Position = new Vector3(0, 0, 5);
		ThirdPersonSpring = spring;

		CallDeferred(MethodName.SetupFreecamNodes);
		
		CameraType = GotoCameraType;
	}


	#endregion

	#region freecam vars

	// freecam adapted from https://github.com/VojtaStruhar/godot-freecam-plugin/blob/main/addons/freecam_3D/freecam.gd

	[ExportGroup("Freecam")]

	[Export] public bool InvertFreecamControls = false;

	[Export] public bool OverlayText = true;


	public Node3D FreecamPivot;

	public VBoxContainer ScreenOverlay;

	public VBoxContainer EventLog;

	public int FreecamMaxSpeed = 4;
	public float FreecamMinSpeed = 0.1f;
	public float FreecamAcceleration = 0.1f;

	[Export]
	public bool FreecamActive = false;

	public float FreecamTargetSpeed;

	[Export]
	public Vector3 FreecamVelocity = Vector3.Zero;

	#endregion
	#region freecam methods

	private void SetupFreecamNodes()
	{
		if (FreecamCamera is null && !Engine.IsEditorHint()) return;

		FreecamTargetSpeed = FreecamMinSpeed;

		EventLog = new();

		FreecamPivot = new()
		{
			Position = Position,
			Rotation = Rotation,
			Name = "FreecamPivot"
		};

		FreecamCamera.AddSibling(FreecamPivot);
		FreecamCamera.Reparent(FreecamPivot);
		FreecamCamera.Position = Vector3.Zero;
		FreecamCamera.Rotation = Vector3.Zero;

		ScreenOverlay = new();
		ScreenOverlay.AddThemeConstantOverride("Separation", 8);
		FreecamCamera.AddChild(ScreenOverlay);
		
		ScreenOverlay.AddChild(MakeLabel("Debug Camera"));
		ScreenOverlay.AddSpacer(false);

		ScreenOverlay.AddChild(EventLog);
		ScreenOverlay.Visible = OverlayText;
	}

	private void FreecamProcess(double delta)
	{
		if (FreecamActive && FreecamCamera is not null && !Engine.IsEditorHint()) {
			Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

			if (Input.IsActionPressed("ui_accept") || Input.IsKeyPressed(Key.E))
			{
			}

			if (inputDir != Vector2.Zero)
			{
				Vector3 forward = -GlobalTransform.Basis.Z;
				forward.Y = 0;
				forward = forward.Normalized();

				Vector3 right = GlobalTransform.Basis.X;
				right.Y = 0;
				right = right.Normalized();

				Vector3 moveDir = (
					right * inputDir.X 
					+ 
					forward * -inputDir.Y
				)
				.Normalized();

				FreecamVelocity = FreecamVelocity.Lerp(moveDir * FreecamTargetSpeed, FreecamAcceleration);
			}
			else
			{
				FreecamVelocity = FreecamVelocity.Lerp(Vector3.Zero * FreecamTargetSpeed, FreecamAcceleration);
			}
		}
	}

	private void FreecamSpeedUp()
	{
		FreecamTargetSpeed = Mathf.Clamp(FreecamTargetSpeed + 0.15f, FreecamMinSpeed, FreecamMaxSpeed);
		DisplayMessage($"[Speed up] {FreecamTargetSpeed}");
	}

	private void FreecamSlowDown()
	{
		FreecamTargetSpeed = Mathf.Clamp(FreecamTargetSpeed - 0.15f, FreecamMinSpeed, FreecamMaxSpeed);
		DisplayMessage($"[Slow up] {FreecamTargetSpeed}");
	}

	private void FreecamInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("freecam") && FreecamCamera is not null)
		{
			FreecamActive ^= true;
			CurrentCamera = FreecamActive ? FreecamCamera : GotoCamera;
			Input.SetMouseMode( FreecamActive ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible);

			if (FreecamActive)
				EmitSignalFreecamEnabled();
			else
				EmitSignalFreecamDisabled();
		}

		if (FreecamActive && FreecamCamera is not null && !Engine.IsEditorHint())
		{
			// Turn around
			if (@event is InputEventMouseMotion motion)
			{
				FreecamPivot.RotateY( - motion.Relative.X * Sensitivity);
				FreecamCamera.RotateX( - motion.Relative.Y * Sensitivity);

				var rot = FreecamCamera.Rotation;
				rot.X = Mathf.Clamp(Rotation.X,  - Mathf.Pi / 2, Mathf.Pi / 2);
				
				FreecamCamera.Rotation = rot;
			}

			if (@event is InputEventMouseButton mouse)
			{
				if (mouse.ButtonIndex == MouseButton.WheelUp && mouse.Pressed)
				{
					if (InvertFreecamControls) 
						FreecamSlowDown(); 
					else 
						FreecamSpeedUp();
				}
				else if (mouse.ButtonIndex == MouseButton.WheelDown && mouse.Pressed)
				{
					if (InvertFreecamControls) 
						FreecamSpeedUp(); 
					else 
						FreecamSlowDown();
				}
			}
		}
	}

	public void DisplayMessage(string text)
	{
		while(EventLog.GetChildCount() >= 3)
		{
			EventLog.RemoveChild(EventLog.GetChild(0));
		}

		EventLog.AddChild(MakeLabel(text));
	}


	private static Label MakeLabel(string text)
		=> new Label
			{
				Text = text
			};

	#endregion

	#region third person cam

	private void ThirdPersonProcess(double delta)
	{
		if (CameraType == CameraTypeEnum.ThirdPerson && !Engine.IsEditorHint())
		{
			Input.MouseMode = (Input.IsActionPressed("camera_move") || ShiftLocked) 
			? Input.MouseModeEnum.Captured 
			: Input.MouseModeEnum.Visible;


			if (Subject is not null && Subject.Head is not null)
			{
				var pos = ThirdPersonSpring.GlobalPosition;
				var newPos = Subject.Head.GlobalPosition;

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
				}

				ThirdPersonSpring.GlobalPosition = pos.Lerp(newPos, 50*(float)delta);
			}

			ThirdPersonSpring.SpringLength = Mathf.Lerp(ThirdPersonSpring.SpringLength, TargetZoom, 20 * (float)delta);	
		}
	}

	private void ThirdPersonUnhandled(InputEvent @event)
	{
		if (CameraType == CameraTypeEnum.ThirdPerson && !Engine.IsEditorHint())
		{
			if (Input.IsActionJustPressed("shift_lock"))
			{
				ShiftLocked ^= true;

				if (ShiftLocked)
					EmitSignalShiftLockEnabled();
				else
					EmitSignalShiftLockDisabled();
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
	}

	private void ThirdPersonInput(InputEvent @event)
	{
		if (
			@event is InputEventMouseMotion mouse && 
				(Input.IsActionPressed("camera_move") || ShiftLocked) // if shiftlocked or holding rmb move camera to mouse relative
				&& Subject is not null 
				&& CameraType == CameraTypeEnum.ThirdPerson 
				&& !Engine.IsEditorHint()
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

	#endregion
	#region events

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Engine.IsEditorHint()) return;

		ThirdPersonProcess(delta);
		FreecamProcess(delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (Engine.IsEditorHint()) return;

		ThirdPersonUnhandled(@event);
	}


	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (Engine.IsEditorHint()) return;

		ThirdPersonInput(@event);
		FreecamInput(@event);
	}

	#endregion
}
