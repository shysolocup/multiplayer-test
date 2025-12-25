using Godot;
using System;
using Core;
using System.Threading.Tasks;
using System.Linq;


[GlobalClass, Icon("uid://fmiylsiygwmg")]
public partial class Characters : Singleton3D<Characters>
{	
	[Export]
	public Character StarterCharacter;

	[Signal]
	public delegate void CharacterSpawnedEventHandler(Character character);

	[Signal]
	public delegate void CharacterRemovedEventHandler(Character character);


	#region character physics
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		var player = Client.LocalPlayer;
		var chara = player?.GetCharacter();

		if (chara is not null )
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

			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * chara.Speed;
				velocity.Z = direction.Z * chara.Speed;
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
		var replication = await GlobalStorage.Instance();
		
		StarterCharacter ??= replication.GetNode<Character>("./starterCharacter");

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

		GD.PushWarning("spawned dummy, is server?: ", inst.Multiplayer.IsServer());

		inst.CallDeferred(Node.MethodName.AddChild, inst);

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
		player.EmitSignal(Player.SignalName.Spawned, character);

		GD.PushWarning($"spawned {player.GetPlayerName()}'s character, is server?: ", player.Multiplayer.IsServer());

		return character;
	}


	public static async Task<Character> GetCharacterById(long id)
	{
		return (await Players.GetPlayerById(id))?.GetCharacter();
	}
	
	#endregion
}
