using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;

[GlobalClass, Icon("uid://cyipqyv61ywf6")]
public partial class Players : Singleton<Players>
{
	[Export]
	public Player StarterPlayer;


	[Signal]
	public delegate void PlayerJoinedEventHandler(Player player);
	[Signal]
	public delegate void PlayerLeftEventHandler(Player player);
	[Signal]
	public delegate void LocalPlayerChangedEventHandler(Player player);


	public override async void _Ready()
	{
		var replication = await GlobalStorage.Instance();
		
		StarterPlayer ??= replication.GetNode<Player>("./starterPlayer");

		ChildEnteredTree += node => {
			if (node is Player player)
			{
				EmitSignalPlayerJoined(player);
			}
		};

		ChildExitingTree += node => {
			if (node is Player player)
			{
				EmitSignalPlayerLeft(player);
			}
		};
	}

	#region replicated

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Replicated Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	/// <summary>
	/// Makes a new player and adds it to the player list
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<Player> MakePlayer()
	{
		var inst = await Instance();

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!inst.Multiplayer.IsServer()) throw new Exception("can't make a player on the client");

		var player = inst.StarterPlayer.Duplicate<Player>();
		player.ProcessMode = ProcessModeEnum.Inherit;

		inst.CallDeferred(Node.MethodName.AddChild, player);
		
		return player;
	}


	/// <summary>
	/// removes a player by node
	/// <para/>@Server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task RemovePlayer(Player player)
	{
		var inst = await Instance();

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!inst.Multiplayer.IsServer()) throw new Exception("can't remove a player on the client");
		
		inst.CallDeferred(Node.MethodName.RemoveChild, player);
	}


	/// <summary>
	/// removes a player by id
	/// <para/>@Server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task RemovePlayer(long id)
	{
		var inst = await Instance();

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!inst.Multiplayer.IsServer()) throw new Exception("can't remove a player on the client");
		
		var player = await GetPlayerById(id);
		inst.CallDeferred(Node.MethodName.RemoveChild, player);
	}


	#endregion
	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Non-Replicated Utility Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	/// <summary>
	/// Gets a player by their peer id
	/// </summary>
	public static async Task<Player> GetPlayerById(long id)
	{
		var inst = await Instance();
		var players = inst.GetChildren<Player>();

		return players.FirstOrDefault(p => p is Player player && player.GetId() == id);
	}

	/// <summary>
	/// Returns an array of players in the game
	/// <para/> Specifically it returns a <see cref="Array"/>
	/// </summary>
	public static async Task<Array<Player>> GetPlayers()
	{
		var inst = await Instance();
		return inst.GetChildren<Player>();
	}

	/// <summary>
	/// Gets a player by their character model
	/// </summary>
	public static async Task<Player> GetPlayerByCharacter(Character character)
	{
		var players = await GetPlayers();
		return players.FirstOrDefault(p => p.GetCharacter() == character);
	}

	#endregion
}
