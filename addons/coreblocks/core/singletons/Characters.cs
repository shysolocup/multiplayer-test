using Godot;
using Godot.Collections;


[GlobalClass, Icon("uid://fmiylsiygwmg")]
public partial class Characters : Singleton3D<Characters>
{	
	[Export]
	public PackedScene StarterCharacter = ResourceLoader.Load<PackedScene>($"res://src/assets/scenes/models/starter_character.tscn", "", ResourceLoader.CacheMode.Replace);

	[Signal]
	public delegate void CharacterSpawnedEventHandler(Character character);

	[Signal]
	public delegate void CharacterRemovedEventHandler(Character character);


	private CameraSystem cameras { get; set; }
	private Workspace workspace { get; set; }
	private Camera3D cam { get; set; }
	private Players players { get; set; }


	[
		Rpc(
			MultiplayerApi.RpcMode.AnyPeer,
			CallLocal = true,
			TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable
		)
	]
	#region character physics
	public void DoPhysics(double delta)
	{
		var player = Client.LocalPlayer;
		var chara = player?.Character;

		// if a. no ui is active b. the character is not null and c. the freecam is not active it'll move the player
		if (this.IsUnhandled() && chara is not null && !cameras.FreecamActive)
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

			if (cameras.ShiftLocked || cameras.CameraType == Enum.CameraType.FirstPerson)
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

				if (!cameras.ShiftLocked && cameras.CameraType != Enum.CameraType.FirstPerson)
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

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		DoPhysics(delta);
	}
	#endregion


	protected private void _spawnEmitter(Node node)
	{
		if (node is Character character)
		{
			EmitSignalCharacterSpawned(character);
		}
	}

	protected private void _removedEmitter(Node node)
	{
		if (node is Character character)
		{
			EmitSignalCharacterRemoved(character);
		}
	}

	protected private Callable _spawnCall => new(this, MethodName._spawnEmitter);
	protected private Callable _remCall => new(this, MethodName._removedEmitter);


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		base._Ready();
		
		cameras = await CameraSystem.Instance();
		workspace = await Workspace.Instance();
		players = await Players.Instance();
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


	#region replicated

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// RPC Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
	public Character Spawn(Player player)
	{
		player.Character?.QueueFree();

		var peerId = player.GetPeerId();
		var id = player.GetId();
		var name = player.GetPlayerName();

		Character character = null;
		var spawner = Replicator.CharacterSpawner;

		if (Game.IsServer())
		{
			character = (Character)spawner.Spawn(new Array { 
				peerId,
				id, 
				name 
			});

			GD.PushWarning($"spawned {name}'s character");
		}

		return character;
	}


	#endregion
	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Non-Replicated Utility Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public Character SpawnDummy(int peerId, string playerId, string playerName)
	{
		var chara = StarterCharacter.Instantiate() as Character;
			chara.GlobalTransform = workspace.Spawn.GlobalTransform;
			chara.Name = playerName;
			chara.SetId(playerId);
			chara.SetMultiplayerAuthority(peerId, true);

		GD.PushWarning("spawned dummy");	

		return chara;
	}


	/// <summary>
	/// Gets a character by their peer id
	/// </summary>
	public Character GetCharacterById(string id)
	{
		var players = GetCharacters();

		foreach (var chara in players)
		{
			if (chara.GetId() == id) return chara;
		}

		return null;
	}


	/// <summary>
	/// Returns an array of characters in the game
	/// <para/> Specifically it returns a <see cref="Array"/>
	/// </summary>
	public Array<Character> GetCharacters()
	{
		var result = new Array<Character>();

		foreach (var node in GetChildren())
		{
			if (node is Character chara) 
				result.Add(chara);
		}

		return result;
	}

	
	#endregion
}
