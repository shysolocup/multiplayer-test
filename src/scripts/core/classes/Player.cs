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
}
