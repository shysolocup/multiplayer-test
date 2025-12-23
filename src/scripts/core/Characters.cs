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


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		var replication = await ReplicationSystem.Instance();
		
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

	#region replicated

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Replicated Methods ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Spawns a character for the given player.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public async static Task<Character> Spawn(Player player)
    {
		player.Character?.QueueFree();

        var character = await MakeCharacter(player);
		
        player.EmitSignal(Player.SignalName.Spawned, character);
		player.Character = character;

        return character;
    }

	#endregion
	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Non-Replicated Utility Methods ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static async Task<Character> MakeCharacter(Player player)
	{
		var inst = await Instance();

		Character character = inst.StarterCharacter.Duplicate() as Character;
		return character;
	}


	public static async Task<Character> GetCharacterById(long id)
	{
		return (await Players.GetPlayerById(id))?.Character;
	}
	
	#endregion
}
