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

	private static Player _localPlayer;

	public static Player LocalPlayer
	{
		get => _localPlayer;
		private set
		{
			_localPlayer?.QueueFree();
			_localPlayer = value;
		}
	}


	[Signal]
	public delegate void PlayerJoinedEventHandler(Player player);
	[Signal]
	public delegate void PlayerLeftEventHandler(Player player);
	[Signal]
	public delegate void LocalPlayerChangedEventHandler(Player player);


	public override async void _Ready()
	{
		var replication = await ReplicationSystem.Instance();
		
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
		var player = inst.StarterPlayer.Duplicate<Player>();

		inst.AddChild(player);
		
		return player;
	}

	/// <summary>
	/// Sets the local player of a player, only available to the server.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static void SetLocalPlayer(Player player)
	{
		LocalPlayer = player;
	}


	#endregion
	#region utility

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Non-Replicated Utility Methods ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static async Task<Player> GetPlayerById(long id)
	{
		var inst = await Instance();
		var players = inst.GetChildren<Player>();

		return players.FirstOrDefault(p => p is Player player && player.PlayerId == id);
	}

	public static async Task<Array<Player>> GetPlayers()
	{
		var inst = await Instance();
		return inst.GetChildren<Player>();
	}

	public static async Task<Player> GetPlayerByCharacter(Character character)
	{
		var players = await GetPlayers();
		return players.FirstOrDefault(p => p.Character == character);
	}

	#endregion
}
