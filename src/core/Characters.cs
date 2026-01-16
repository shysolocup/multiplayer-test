using Godot;
using System.Threading.Tasks;


[GlobalClass, Icon("uid://fmiylsiygwmg")]
public partial class Characters : Singleton3D<Characters>
{	
	[Export]
	public PackedScene StarterCharacter = ResourceLoader.Load<PackedScene>($"res://src/scenes/starter_character.tscn", "", ResourceLoader.CacheMode.Replace);

	[Signal]
	public delegate void CharacterSpawnedEventHandler(Character character);

	[Signal]
	public delegate void CharacterRemovedEventHandler(Character character);

	static private CameraSystem cameras { get; set; }
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

		if (chara is not null && !cameras.FreecamActive)
		{
			Vector3 velocity = chara.Velocity;

			if (!chara.IsOnFloor())
			{
				velocity += chara.GetGravity() * (float)delta;
			}

			if (Input.IsActionPressed("jump") && chara.IsOnFloor())
			{
				velocity.Y = chara.JumpVelocity;
			}

			Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

			if (cameras.ShiftLocked || cameras.CameraType == CameraSystem.CameraTypeEnum.FirstPerson)
			{
				var rot = chara.GlobalRotation;
				
				chara.GlobalRotation = new Vector3(
					rot.X, 
					cam.GlobalRotation.Y + Mathf.Pi, 
					rot.Z
				);

			}

			if (inputDir != Vector2.Zero)
			{
				Vector3 forward = -cam.GlobalBasis.Z;
				forward.Y = 0;
				forward = forward.Normalized();

				Vector3 right = cam.GlobalBasis.X;
				right.Y = 0;
				right = right.Normalized();

				Vector3 moveDir = (right * inputDir.X + forward * -inputDir.Y).Normalized();

				if (!cameras.ShiftLocked && cameras.CameraType != CameraSystem.CameraTypeEnum.FirstPerson)
				{
					float yaw = Mathf.Atan2(moveDir.X, moveDir.Z);
					yaw = Mathf.LerpAngle(chara.Rotation.Y, yaw, (float)delta * 10f);

					chara.Rotation = new Vector3(chara.Rotation.X, yaw, chara.Rotation.Z);		
				}

				velocity.X = moveDir.X * chara.Speed;
				velocity.Z = moveDir.Z * chara.Speed;
			}

			else
			{
				velocity.X = Mathf.MoveToward(chara.Velocity.X, 0, chara.Speed);
				velocity.Z = Mathf.MoveToward(chara.Velocity.Z, 0, chara.Speed);
			}

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
		
		cameras = await CameraSystem.Instance();
		cam = cameras.CurrentCamera;
		
		StarterCharacter ??= ResourceLoader.Load<PackedScene>($"res://src/scenes/starter_character.tscn", "", ResourceLoader.CacheMode.Replace);

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

		Character character = inst.StarterCharacter.Instantiate() as Character;
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
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]
	public async static Task<Character> Spawn(Player player)
	{
		player.GetCharacter()?.QueueFree();

		var character = await SpawnDummy(player);

		player.SetCharacter(character);
		player.EmitSignal(Player.SignalName.Spawned, character);

		GD.PushWarning($"spawned {player.GetPlayerName()}'s character");

		cameras.SetSubject(player);

		return character;
	}


	public static async Task<Character> GetCharacterById(string id)
	{
		return (await Players.GetPlayerById(id))?.GetCharacter();
	}
	
	#endregion
}
