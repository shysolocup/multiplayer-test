using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace Core
{

	#region NodeTunnelBridge

	public static class NodeTunnelBridge
	{

		public static void ConnectToRelay(MultiplayerPeer peer, string nodeTunnelAddress, int nodeTunnelPort) => 
			peer.Call("connect_to_relay", nodeTunnelAddress, nodeTunnelPort);

		public static void Host(MultiplayerPeer peer) =>
			peer.Call("host");

		public static void Join(MultiplayerPeer peer, string hostId) =>
			peer.Call("join", hostId);

		public static async Task RelayConnected(MultiplayerPeer peer) => 
			await peer.ToSignal(peer, "relay_connected");

		public static async Task Hosting(MultiplayerPeer peer) => 
			await peer.ToSignal(peer, "hosting");

		public static async Task Joined(MultiplayerPeer peer) =>
			await peer.ToSignal(peer, "joined");

		public static string GetOnlineId(MultiplayerPeer peer) => 
			peer.Get<string>("online_id");

		public static MultiplayerPeer NewPeer()
		{
			var script = GD.Load<GDScript>("res://addons/nodetunnel/NodeTunnelPeer.gd");
			return (MultiplayerPeer)script.New();
		}
	}

	#endregion
	#region Chore

	public static class Chore
	{
		/// <summary>
		/// Creates a new thread and deletes the given node after a given time.
		/// </summary>
		public static CancellationTokenSource Debris(Node node, float time)
		{
			return Delay(time, async token =>
			{
				if (!token.IsCancellationRequested)
				{
					node.QueueFree();
				}
			});
		}

		/// <summary>
		/// Waits for a given time
		/// </summary>
		/// <param name="time">time in milliseconds to wait for</param>
		public async static Task Sleep(float time)
		{
			Game game = await Game.Instance();
			SceneTreeTimer t = game.GetTree().CreateTimer(time);
			await game.ToSignal(t, SceneTreeTimer.SignalName.Timeout);
			t.Dispose();
		}


		/// <summary>
		/// Waits for a given time and runs function in a new thread
		/// </summary>
		/// <param name="time">time in milliseconds to wait for</param>
		public static CancellationTokenSource Delay(float time, Func<CancellationToken, Task> callback)
		{

			var cts = new CancellationTokenSource();

			Task.Run(async () =>
			{
				var game = await Game.Instance();
				SceneTreeTimer t = game.GetTree().CreateTimer(time);

				await game.ToSignal(t, SceneTreeTimer.SignalName.Timeout);
				await callback(cts.Token);

				t.Dispose();
			});

			return cts;
		}

		/// <summary>
		/// Waits for a given time and runs function in a new thread
		/// </summary>
		/// <param name="time">time in milliseconds to wait for</param>
		public static void Delay(float time, Func<Task> callback) => Task.Run(async () =>
			{
				var game = await Game.Instance();
				SceneTreeTimer t = game.GetTree().CreateTimer(time);

				await game.ToSignal(t, SceneTreeTimer.SignalName.Timeout);
				await callback();

				t.Dispose();
			});

		/// <summary>
		/// Spawns a new cancellable threaded task
		/// </summary>
		/// <param name="callback">The callback to run</param>
		/// <returns>CancellationTokenSource to cancel the task</returns>
		public static CancellationTokenSource Spawn(Func<CancellationToken, Task> callback)
		{
			var cts = new CancellationTokenSource();

			Task.Run(async () =>
			{
				await callback(cts.Token);
			});

			return cts;
		}

		/// <summary>
		/// Spawns a new cancellable threaded task
		/// </summary>
		/// <param name="callback">The callback to run</param>
		/// <returns>CancellationTokenSource to cancel the task</returns>
		public static void Spawn(Func<Task> callback) => Task.Run(async () =>
			{
				await callback();
			});
	}
	#endregion


	#region Util
	public static class Util
	{
		public static Variant BlankVariant = new Variant();

		public static (TValue, int)[] Enumerate<[MustBeVariant] TValue>(Array<TValue> arr)
		{
			return [.. arr.ToArray().Select((value, i) => (value, i))];
		}
	}
	#endregion


	// jsonc converter 
	#region Jsonc
	public static partial class Jsonc {
		public static string Minify(string jsonc)
		{
			return LineCommentGuh().Replace(BlockCommentGuh().Replace(jsonc, ""), "");
		}


		[GeneratedRegex(@"//.*?$", RegexOptions.Multiline)]
		private static partial Regex LineCommentGuh();


		[GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
		private static partial Regex BlockCommentGuh();
	}
	#endregion

}
