using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class Ext : Node {}


public static class Extensions
{
	public static T GetGameNode<T>(this Node self, NodePath nodePath)  where T : Node
	{
		return self.GetNode<T>($"/root/Game/{nodePath}");
	}
	public static Node GetGameNode(this Node self, NodePath nodePath) => self.GetGameNode<Node>(nodePath);


	#nullable enable
	public static T? GetAncestorOfType<T>(this Node self, int layers = 100) where T : Node?
	{
		Node current = self;
		for (int i = 0; i < layers; i++)
		{
			current = current.GetParentOrNull<Node>();
			if (current is T ret) return ret;
		}

		return null;
	}
	#nullable restore


	public static Array<T> GetChildren<[MustBeVariant] T>(this Node self) where T : Node
	{
		return self.GetChildren() as Array<T>;
	}


	public static Array<T> GetDescendants<[MustBeVariant] T>(this Node self) where T : Node
	{
		Array<T> descendants = [];

		foreach (Node child in self.GetChildren())
		{
			if (child is T node)
			{
				descendants.Add(node);
				descendants += node.GetDescendants<T>();
			}
		}

		return descendants;
	}
	public static Array<Node> GetDescendants(this Node self) => self.GetDescendants<Node>();


	public static void ClearChildren(this Node self)
	{
		foreach (Node child in self.GetChildren()) {
			self.RemoveChild(child);
			child.QueueFree();
		}
	}


	public static Array<T> GetChildrenOfType<[MustBeVariant] T>(this Node self) where T : Node => self.FindChildren("*", typeof(T).Name) as Array<T>;


	public static void ClearChildren<T>(this Node self) where T : Node
	{
		foreach (Node child in self.GetChildren()) {
			if (child.GetType() == typeof(T)) {
				self.RemoveChild(child);
				child.QueueFree();
			}
		}
	}

	public static T FindChild<T>(this Node self, string pattern) where T : Node
	{
		if (pattern == "T") pattern = typeof(T).Name;
		return self.FindChild(pattern) as T;
	}

	public static async Task Guh(this SceneTreeTimer self) {
		await self.ToSignal(self, Timer.SignalName.Timeout);
		self.Dispose();
	}

	public static T Duplicate<T>(this Node self) where T : Node => self.Duplicate() as T;

	public static string ToHex(this Color self, string end = "0x") {
		(double r, double g, double b) = ( Mathf.Floor(self.R*255), Mathf.Floor(self.G*255), Mathf.Floor(self.B*255) );
		return string.Format( end+"{0:x}{1:x}{2:x}", (int)r, (int)g, (int)b);
	}

	public static System.Collections.Generic.Dictionary<string, object> Blanks = [];

	public static T GetBlank<T>(this GodotObject self) where T : class 
	{
		string name = typeof(T).Name;
		if (!Blanks.ContainsKey(name)) {
			T thing = Activator.CreateInstance<T>();
			Blanks[name] = thing;
		}
		return (T)Blanks[name];
	}

	/// <summary>
	/// Base dynamic method of linear interpolation without any special easing arguments
	/// </summary>
	/// <param name="value">Original value before lerping</param>
	/// <param name="goal">Goal of the lerp</param>
	/// <param name="_t">Factor from 0 to 1 usually done by dividing 1 by a float (eg: 1/1.5f)</param>
	/// <returns>New value after doing stupid math</returns>
	public static dynamic BaseLerp(this GodotObject self, dynamic value, dynamic goal, float _t, dynamic delta = null) 
	{
		if (delta != null) _t = self.FactorDelta(_t, (double)delta);
		return value + (goal - value) * _t;
	}


	public static float FactorDelta(this GodotObject self, float _t, double delta) => _t / ((1/(float)delta) / (float)Performance.GetMonitor(Performance.Monitor.TimeFps));


	/// <summary>
	/// Dynamic method of linear interpolation functions with different methods of easing. It's meant to be used along with _Process that way it can smoothly transition to the goal
	/// </summary>
	/// <param name="value">Original value before lerping</param>
	/// <param name="goal">Goal of the lerp</param>
	/// <param name="time">Time in seconds that the lerp takes</param>
	/// <param name="trans">Transition type defaulting to Tween.TransitionType.Linear</param>
	/// <param name="ease">Transition easing defaulting to Tween.EaseType.InOut</param>
	/// <returns>New value after doing stupid math</returns>
	public static dynamic Twlerp(this GodotObject self, dynamic value, dynamic goal, float _t, double delta, Tween.TransitionType trans = Tween.TransitionType.Linear, Tween.EaseType ease = Tween.EaseType.InOut)
	{
		_t = self.FactorDelta(_t, delta);

		float e = _t;

		switch(trans) 
		{
			case Tween.TransitionType.Sine: {

				switch(ease) 
				{
					case Tween.EaseType.In:
						e = (float)(1 - Math.Cos( _t * Math.PI / 2));
						break;

					case Tween.EaseType.Out:
						e = (float)Math.Sin( _t * Math.PI / 2 );
						break;

					case Tween.EaseType n when n == Tween.EaseType.InOut || n == Tween.EaseType.OutIn:
						e = (float)(-(Math.Cos(Math.PI * _t) - 1) / 2);
						break;

				} break;
			}

			case Tween.TransitionType.Quad: {
				switch(ease) 
				{
					case Tween.EaseType.In:
						e = (float)Math.Pow(_t, 2);
						break;

					case Tween.EaseType.Out:
						e = (float)(1 - Math.Pow(1 - _t, 2));
						break;

					case Tween.EaseType n when n == Tween.EaseType.InOut || n == Tween.EaseType.OutIn:
						e = (float)((_t < 0.5f) ? 2 * Math.Pow(_t, 2) : 1 - Math.Pow(-2 * _t + 2, 2) / 2);
						break;
				} break;
			}

			case Tween.TransitionType.Cubic: {
				switch(ease) 
				{
					case Tween.EaseType.In:
						e = (float)Math.Pow(_t, 3);
						break;

					case Tween.EaseType.Out:
						e = (float)(1 - Math.Pow(1 - _t, 3));
						break;

					case Tween.EaseType n when n == Tween.EaseType.InOut || n == Tween.EaseType.OutIn:
						e = (float)((_t < 0.5f) ? 4 * Math.Pow(_t, 3) : 1 - Math.Pow(-2 * _t + 2, 3) / 2);
						break;
				} break;
			}

			case Tween.TransitionType.Quart: {
				switch(ease) 
				{
					case Tween.EaseType.In:
						e = (float)Math.Pow(_t, 4);
						break;

					case Tween.EaseType.Out:
						e = (float)(1 - Math.Pow(1 - _t, 4));
						break;

					case Tween.EaseType n when n == Tween.EaseType.InOut || n == Tween.EaseType.OutIn:
						e = (float)((_t < 0.5f) ? 8 * Math.Pow(_t, 4) : 1 - Math.Pow(-2 * _t + 2, 4) / 2);
						break;
				} break;
			}
		}

		return BaseLerp(self, value, goal, e);
	}

	public static Vector2 Snapped(this Vector2 vector, Vector2 gridSize)
	{
		return new Vector2(
			Mathf.Floor(vector.X / gridSize.X) * gridSize.X,
			Mathf.Floor(vector.Y / gridSize.Y) * gridSize.Y
		);
	}

	/// <summary>
	/// Retrieves a slice of the string, split by the given delimiter.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="delimiter">The delimiter character.</param>
	/// <param name="index">The zero-based index of the slice to retrieve.</param>
	/// <returns>The slice at the specified index or an empty string if the index is out of range.</returns>
	public static string GetSlice(this string source, char delimiter, int index)
	{
		var slices = source.Split(delimiter);
		return (index >= 0 && index < slices.Length) ? slices[index] : string.Empty;
	}

	/// <summary>
	/// Checks if all elements in the array satisfy the given condition.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	/// <param name="array">The array to evaluate.</param>
	/// <param name="predicate">The condition to evaluate for each element.</param>
	/// <returns>True if all elements satisfy the condition, otherwise false.</returns>
	public static bool All<T>(this Godot.Collections.Array array, Func<T, bool> predicate)
	{
		foreach (var item in array)
		{
			if (item is T t && !predicate(t))
			{
				return false;
			}
		}
		return true;
	}
	
	public static async Task<T> GetNodeAsync<T>(this Node self, string nodeName) where T : class
	{
		int elapsed = 0;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}

		return self.GetNode<T>(nodeName);
	}

	public static async Task<Node> GetNodeAsync(this Node self, string nodeName) => await self.GetNodeAsync<Node>(nodeName);

	public static async Task WaitUntilNodeReady(this Node self, string nodeName)
	{
		int elapsed = 0;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}
	}

	public async static Task WaitUntilPropertyReady(this Node self, string propertyName)
	{
		int elapsed = 0;
		
		while ((object)self.Get(propertyName) == null) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= 5000) {
				throw new TimeoutException();
			}
		}
	}
}
