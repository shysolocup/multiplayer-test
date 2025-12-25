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


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		var replication = await GlobalStorage.Instance();
		
		StarterCharacter ??= replication.GetNode<Character>("./starterCharacter");

		ChildEnteredTree += node => {
			if (node is Character character)
			{
				EmitSignalCharacterSpawned(character);
			}
		};

		ChildExitingTree += node => {
			if (node is Character character)
			{
				EmitSignalCharacterRemoved(character);
			}
		};
	}

	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Non-Replicated Utility Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<Character> MakeCharacter(Player player)
	{
		var inst = await Instance();
		var workspace = await Workspace.Instance();

		Character character = inst.StarterCharacter.Duplicate() as Character;
		character.GlobalTransform = workspace.Spawn.GlobalTransform;
		character.Name = player.GetPlayerName();

		player.SetCharacter(character);

		inst.CallDeferred(Node.MethodName.AddChild, inst);

		return character;
	}


	public static async Task<Character> GetCharacterById(long id)
	{
		return (await Players.GetPlayerById(id))?.GetCharacter();
	}
	
	#endregion
}
