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
	[Export] private string Id { get; set; }
	[Export] private long PeerId { get; set; }
	[Export] public Character Character
	{
		get => characters is Characters c ? c.GetCharacterById(Id) : null;
		set => throw new System.Exception("don't do that");
	}

	public string GetId() => Id;
	public long GetPeerId() => PeerId;
	public string GetPlayerName() => PlayerName;


	private Characters characters { get; set; }


    public override async void _Ready()
    {
        base._Ready();

		characters = await Characters.Instance();
    }



	/// <summary>
	/// sets the player's peer id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void SetPlayerName(string name)
	{
		PlayerName = name;
		Name = name;
	}


	/// <summary>
	/// sets the player's peer id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void SetPlayerId(string id)
	{
		Id = id;
	}


	/// <summary>
	/// sets the player's peer id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void SetPeerId(long id)
	{
		PeerId = id;
	}
}
