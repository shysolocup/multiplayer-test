using System.Threading.Tasks;
using Godot;


[GlobalClass, Icon("uid://dj1gftejreec6")]
public partial class Player : Node
{
	/// <summary>
	/// Event for when the character is created
	/// </summary>
	/// <param name="character"></param>
	[Signal]
	public delegate void SpawnedEventHandler(Character character);

	[Export] private string PlayerName { get; set; } = "Player";
	[Export] private long Id { get; set; }
	[Export] private Character Character;

	public long GetId() => Id;
	public string GetPlayerName() => PlayerName;
	public Character GetCharacter() => Character;


	/// <summary>
	/// sets the player's peer id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetPlayerName(string name)
	{
		if (Multiplayer.IsServer())
		{
			PlayerName = name;	
		}
	}


	/// <summary>
	/// sets the player's peer id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetPlayerId(long id)
	{
		if (Multiplayer.IsServer())
		{
			Id = id;	
		}
	}

	/// <summary>
	/// sets the player's character
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetCharacter(Character character)
	{
		if (Multiplayer.IsServer())
		{
			Character = character;	
		}
	}
	

	/// <summary>
	/// Spawns a character for the given player.
	/// <para/>@server
	/// </summary>
	public override async void _Ready()
	{
		base._Ready();

		if (GetParent() is not Players)
		{
			var players = await Players.Instance();
			players.AddChild(this);
		}
	}


	/// <summary>
	/// Spawns a character for the given player.
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async static Task<Character> Spawn(Player player)
	{
		player.Character?.QueueFree();

		var character = await Characters.MakeCharacter(player);
		
		player.EmitSignal(SignalName.Spawned, character);
		player.Character = character;

		return character;
	}
}
