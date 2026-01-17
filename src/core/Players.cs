using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;

/// <summary>
/// replicated singleton that contains and manages players
/// </summary>
[GlobalClass, Icon("uid://cyipqyv61ywf6")]
public partial class Players : Singleton<Players>
{
	[Export]
	public PackedScene StarterPlayer = ResourceLoader.Load<PackedScene>($"res://src/scenes/starter_player.tscn", "", ResourceLoader.CacheMode.Replace);

	private Characters characters { get; set; }
	private Client client { get; set; }

	[Export]
	public Player LocalPlayer
	{
		get => Client.LocalPlayer;
		set {}
	}


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
		base._Ready();

		characters = await Characters.Instance();
		client = await Client.Instance();
		
		StarterPlayer ??= ResourceLoader.Load<PackedScene>($"res://src/scenes/starter_player.tscn", "", ResourceLoader.CacheMode.Replace);

		Connect(Node.SignalName.ChildEnteredTree, _joinedCall);
		Connect(Node.SignalName.ChildExitingTree, _leftCall);
	}



	#region replicated

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// RPC Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Makes a new player and adds it to the player list
	/// </summary>
	
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true, 
			TransferMode = MultiplayerPeer.TransferModeEnum.Reliable
		)
	]

	public Player MakePlayer(long peerId, string id)
	{
		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		/// if (!Game.IsServer()) throw new Exception("can't make a player on the client");
		

		/* 
			INCASE THEY SOMEHOW GET PAST THE TWO OTHER RESTRICTIONS becuase I'm paranoid as SHIT
			the mere CHANCE that a little RAT could send an event to join a second time and break the entire thing
			FUCK YOUUUU
		*/
		var CHEATING_FUCKING_RAT = GetPlayerById(id);
		

		if (CHEATING_FUCKING_RAT is not null)
		{
			throw new Exception("YOU LITTLE FUCKIN RAT STOP CHEATING TRYING TO MAKE A NEW PLAYER AND BREAK MY ENGINE YOU PIECE OF SHIT GET OUT OF MY WALLSSSSS");
		}

		var player = (Player)StarterPlayer.Instantiate();
			player.ProcessMode = ProcessModeEnum.Inherit;
			player.SetPeerId(peerId);
			player.SetPlayerId(id);
			player.SetPlayerName($"player:${id}");

		GD.PushWarning("spawned player, is server?: ", !Game.IsServer());

		client.SetLocalPlayer(player);
		characters.Spawn(player);
		
		return player;
	}


	/// <summary>
	/// removes a player by node
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = true
		)
	]

	public void RemovePlayer(Player player)
	{

		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!Game.IsServer()) throw new Exception("can't remove a player on the client");
		
		CallDeferred(Node.MethodName.RemoveChild, player);
	}


	/// <summary>
	/// removes a player by id
	/// <para/>@server
	/// </summary>
	[
		Rpc(
			MultiplayerApi.RpcMode.Authority, 
			CallLocal = false
		)
	]

	public void RemovePlayer(string id)
	{
		// incase they somehow get past the rpc restriction bc I lowk don't know how it works
		if (!Game.IsServer()) throw new Exception("can't remove a player on the client");
		
		var player = GetPlayerById(id);
		CallDeferred(Node.MethodName.RemoveChild, player);
	}


	#endregion
	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Non-Replicated Utility Methods ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	/// <summary>
	/// Gets a player by their peer id
	/// </summary>
	public Player GetPlayerById(string id)
	{
		var players = GetPlayers();

		foreach (var player in players)
		{
			if (player.GetId() == id) return player;
		}

		return null;
	}

	/// <summary>
	/// Returns an array of players in the game
	/// <para/> Specifically it returns a <see cref="Array"/>
	/// </summary>
	public Array<Player> GetPlayers()
	{
		var result = new Array<Player>();

		foreach (var node in GetChildren())
		{
			if (node is Player player) 
				result.Add(player);
		}

		return result;
	}

	/// <summary>
	/// Gets a player by their character model
	/// </summary>
	public Player GetPlayerByCharacter(Character character)
	{
		var players = GetPlayers();
		foreach (var player in players)
		{
			if (player.GetCharacter() == character) return player;
		}

		return null;
	}

	#endregion
}
