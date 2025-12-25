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


	private void _joinedEmitter(Node node)
	{
		if (node is Player player)
		{
			EmitSignalPlayerJoined(player);
		}
	}

	private void _leftEmitter(Node node)
	{
		if (node is Player player)
		{
			EmitSignalPlayerLeft(player);
		}
	}

	private Callable _joinedCall => new(this, MethodName._joinedEmitter);
	private Callable _leftCall => new (this, MethodName._leftEmitter);


	public override async void _Ready()
	{
		var replication = await GlobalStorage.Instance();
		
		StarterPlayer ??= replication.GetNode<Player>("./starterPlayer");

		Connect(Node.SignalName.ChildEnteredTree, _joinedCall);
		Connect(Node.SignalName.ChildExitingTree, _leftCall);
	}

    public override void _ExitTree()
    {
        base._ExitTree();
		Disconnect(Node.SignalName.ChildEnteredTree, _joinedCall);
		Disconnect(Node.SignalName.ChildExitingTree, _leftCall);
    }



	#region replicated

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// RPC Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	/// <summary>
	/// Makes a new player and adds it to the player list
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<Player> MakePlayer(long id)
	{
		var inst = await Instance();

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!inst.Multiplayer.IsServer()) throw new Exception("can't make a player on the client");
		
		// INCASE THEY SOMEHOW GET PAST THE TWO OTHER RESTRICTIONS becuase I'm paranoid as SHIT
		// the mere CHANCE that a little RAT could send an event to join a second time and break the entire thing
		// FUCK YOUUUU
		// herherherherherher heeeehehehehe
		var CHEATING_FUCKING_RAT = await GetPlayerById(id);
		
		if (CHEATING_FUCKING_RAT is not null)
		{
			throw new Exception("YOU LITTLE FUCKIN RAT STOP CHEATING TRYING TO MAKE A NEW PLAYER AND BREAK MY ENGINE YOU PIECE OF SHIT GET OUT OF MY WALLSSSSS");
		}

		var player = inst.StarterPlayer.Duplicate<Player>();
		player.ProcessMode = ProcessModeEnum.Inherit;
		player.SetPlayerId(id);
		player.SetPlayerName($"player:${id}");

		inst.CallDeferred(Node.MethodName.AddChild, player);

		GD.PushWarning("spawned player, is server?: ", inst.Multiplayer.IsServer());

		await Characters.Spawn(player);
		
		return player;
	}


	/// <summary>
	/// removes a player by node
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
	public static async Task RemovePlayer(Player player)
	{
		var inst = await Instance();

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!inst.Multiplayer.IsServer()) throw new Exception("can't remove a player on the client");
		
		inst.CallDeferred(Node.MethodName.RemoveChild, player);
	}


	/// <summary>
	/// removes a player by id
	/// <para/>@server
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
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
