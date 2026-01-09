using Godot;
using System.Threading.Tasks;


[GlobalClass, Icon("uid://fmiylsiygwmg")]
public partial class Characters : Singleton3D<Characters>
{	
	[Export]
	public Character StarterCharacter;

	[Signal]
	public delegate void CharacterSpawnedEventHandler(Character character);

	[Signal]
	public delegate void CharacterRemovedEventHandler(Character character);

	private CameraSystem cameras { get; set; }
	private Camera3D cam { get; set; }


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable
		)
	]
	#region character physics
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var player = Client.LocalPlayer;
		var chara = player?.GetCharacter();

		if (chara is not null)
		{
			Vector3 velocity = chara.Velocity;

			if (!chara.IsOnFloor())
			{
				velocity += chara.GetGravity() * (float)delta;
			}

			if (Input.IsActionJustPressed("ui_accept") && chara.IsOnFloor())
			{
				velocity.Y = chara.JumpVelocity;
			}

			Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			Vector3 direction = (chara.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

			if (inputDir != Vector2.Zero)
			{
				// Camera forward/right
				Vector3 camForward = -cam.GlobalTransform.Basis.Z;
				camForward.Y = 0;
				camForward = camForward.Normalized();

				Vector3 camRight = cam.GlobalTransform.Basis.X;
				camRight.Y = 0;
				camRight = camRight.Normalized();

				// Flip Y input so forward moves forward
				Vector3 moveDir = (camRight * inputDir.X + camForward * -inputDir.Y).Normalized();

				// Rotate character to face movement direction
				float yaw = Mathf.Atan2(moveDir.X, moveDir.Z); // correct signs
				yaw = Mathf.LerpAngle(chara.Rotation.Y, yaw, (float)delta * 10f);

				chara.Rotation = new Vector3(chara.Rotation.X, yaw, chara.Rotation.Z);

				// Apply movement
				velocity.X = moveDir.X * chara.Speed;
				velocity.Z = moveDir.Z * chara.Speed;
			}


			
			else
			{
				velocity.X = Mathf.MoveToward(chara.Velocity.X, 0, chara.Speed);
				velocity.Z = Mathf.MoveToward(chara.Velocity.Z, 0, chara.Speed);
			}

			GD.Print(velocity);

			chara.Velocity = velocity;
			chara.MoveAndSlide();		
		}	
	}
	#endregion


	private void _spawnEmitter(Node node)
	{
		if (node is Character character)
		{
			EmitSignalCharacterSpawned(character);
		}
	}

	private void _removedEmitter(Node node)
	{
		if (node is Character character)
		{
			EmitSignalCharacterRemoved(character);
		}
	}

	private Callable _spawnCall => new(this, MethodName._spawnEmitter);
	private Callable _remCall => new(this, MethodName._removedEmitter);


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();
		
		var storage = await GlobalStorage.Instance();
		cameras = await CameraSystem.Instance();
		cam = cameras.Camera;
		
		StarterCharacter ??= storage.GetNode<Character>("./starterCharacter");

		Connect(Node.SignalName.ChildEnteredTree, _spawnCall);
		Connect(Node.SignalName.ChildExitingTree, _remCall);
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		// dotnet security
		Disconnect(Node.SignalName.ChildEnteredTree, _spawnCall);
		Disconnect(Node.SignalName.ChildExitingTree, _remCall);
	}


	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Non-Replicated Utility Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	private static async Task<Character> SpawnDummy(Player player)
	{

		var inst = await Instance();
		var workspace = await Workspace.Instance();

		Character character = inst.StarterCharacter.Duplicate() as Character;
		character.GlobalTransform = workspace.Spawn.GlobalTransform;
		character.Name = player.GetPlayerName();

		GD.PushWarning("spawned dummy");

		inst.CallDeferred(Node.MethodName.AddChild, character);

		return character;  
	}


	/// <summary>
	/// Spawns a character for the given player.
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async static Task<Character> Spawn(Player player)
	{
		player.GetCharacter()?.QueueFree();

		var character = await SpawnDummy(player);

		player.SetCharacter(character);
		CameraSystem.SetTarget(character);

		player.EmitSignal(Player.SignalName.Spawned, character);

		GD.PushWarning($"spawned {player.GetPlayerName()}'s character");

		return character;
	}


	public static async Task<Character> GetCharacterById(string id)
	{
		return (await Players.GetPlayerById(id))?.GetCharacter();
	}
	
	#endregion
}
