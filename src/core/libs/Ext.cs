using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

// ignore this
public partial class Ext : Node {}


/// <summary>
/// extension lib class I kinda made that allows for more methods for roblox like stuff
/// <para/> guh I'm tired ngl
/// </summary>
public static class NodeE
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region GetNodeOfType

	/// <summary>
    /// <para>Gets the first child of the <see cref="Node"/> matching the given generic type or class.</para>
	/// <para><code>
	/// var label = container.GetNodeOfType&lt;Label&gt;(); // get the first label in the container
	/// </code></para>
    /// </summary>
	public static T GetNodeOfType<T>(this Node self) where T : Node
		=> self.FindChild<T>();

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Get

    /// <summary>
    /// <para>Returns the <see cref="Variant"/> value of the given <paramref name="property"/> as the given generic type. If the <paramref name="property"/> does not exist, this method returns <see langword="null"/>.</para>
    /// <para><code>
    /// var node = new Node2D();
    /// node.Rotation = 1.5f;
    /// var a = node.Get&lt;float&gt;(Node2D.PropertyName.Rotation); // a is 1.5
    /// </code></para>
    /// <para><b>Note:</b> In C#, <paramref name="property"/> must be in snake_case when referring to built-in Godot properties. Prefer using the names exposed in the <c>PropertyName</c> class to avoid allocating a new <see cref="StringName"/> on each call.</para>
    /// </summary>
    public static T Get<[MustBeVariant] T>(this GodotObject self, StringName property)
		=> self.Get(property).As<T>();

    #endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    #region GetAncestorOfType

    /// <summary>
    /// <para>Gets the first ancestor of the <see cref="Node"/> matching the given generic type or class.</para>
	/// <para><code>
	/// var container = label.GetAncestorOfType&lt;VBoxContainer&gt;(); // get the container from the label
	/// </code></para>
    /// </summary>
    public static T GetAncestorOfType<T>(this Node self) where T : Node
	{
		Node current = self;
		while (true) {
			current = current.GetParentOrNull<Node>();

			if (current is null) break;
			else if (current is T ret) return ret;
		}

		return null;
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region GetChildren

	/// <summary>
    /// <para>Gets all children matching the given generic type.</para>
	/// <para><code>
	/// var labels = container.GetChildren&lt;Label&gt;();
	/// </code></para>
    /// </summary>
	public static Array<T> GetChildren<[MustBeVariant] T>(this Node self) where T : Node
		=> Variant.From(
			self
				.GetChildren()
				.Select( c => c is T t ? t : null)
				.Where( c => c != null)
				.ToArray()
			)
			.AsGodotArray<T>();

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region GetDescendants

	/////// GENERIC ///////
	/// <summary>
    /// <para>Gets all descendants matching the given generic type.</para>
	/// <para><code>
	/// var labels = container.GetDescendants&lt;Label&gt;();
	/// </code></para>
    /// </summary>
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


	/////// DEFAULT ///////
	/// <summary>
    /// <para>Gets all descendants</para>
	/// <para><code>
	/// var descendants = workspace.GetDescendants();
	/// </code></para>
    /// </summary>
	public static Array<Node> GetDescendants(this Node self) 
		=> self.GetDescendants<Node>();

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region ClearChildren

	/////// DEFAULT ///////
	/// <summary>
    /// <para>Removes all children from the node.</para>
    /// </summary>
	public static void ClearChildren(this Node self)
	{
		foreach (Node child in self.GetChildren()) {
			self.RemoveChild(child);
			child.QueueFree();
		}
	}


	/////// GENERIC ///////
	/// <summary>
    /// <para>Removes all children from the node matching the given generic type.</para>
	/// <para><code>
	/// container.ClearChildren&lt;Label&gt;(); // remove all labels from the container
	/// </code></para>
    /// </summary>
	public static void ClearChildren<T>(this Node self) where T : Node
	{
		foreach (Node child in self.GetChildren()) {
			if (child is T) {
				self.RemoveChild(child);
				child.QueueFree();
			}
		}
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region GetChildrenOfType

	/// <summary>
    /// <para>Gets and returns a list of all children matching the given genric type.</para>
    /// </summary>
	public static Array<T> GetChildrenOfType<[MustBeVariant] T>(this Node self) where T : Node 
		=> self.FindChildren("*", typeof(T).Name) as Array<T>;

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region FindChild

	/// <summary>
	/// I can't be bothered to write something actually good for this maybe I will later
	/// <para/> it just casts to T but if you put T in the pattern it'll find the child that is a T
	/// </summary>
	public static T FindChild<T>(this Node self, string pattern = "T") where T : Node
	{
		if (pattern == "T") pattern = typeof(T).Name;
		return self.FindChild(pattern) as T;
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region Duplicate

	/// <summary>
    /// <para>Duplicates the node casting as the given generic type, returning a new node with all of its properties, signals, groups, and children copied from the original. The behavior can be tweaked through the <paramref name="flags"/> (see <see cref="Godot.Node.DuplicateFlags"/>). Internal nodes are not duplicated.</para>
    /// <para><b>Note:</b> For nodes with a <see cref="Godot.Script"/> attached, if <see cref="Godot.GodotObject.GodotObject()"/> has been defined with required parameters, the duplicated node will not have a <see cref="Godot.Script"/>.</para>
    /// </summary>
	public static T Duplicate<T>(this Node self) where T : Node 
		=> self.Duplicate() as T;

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region WaitForNode

	/////// GENERIC ///////
	/// <summary>
	/// Wait until a node is ready to get it
	/// </summary>
	public static async Task<T> WaitForNode<T>(this Node self, string nodeName, int timeout = 5) where T : class
	{
		int elapsed = 0;
		int to = timeout * 1000;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= to) {
				throw new TimeoutException();
			}
		}

		return self.GetNode<T>(nodeName);
	}

	/////// DEFAULT ///////
	/// /// <summary>
	/// Wait until a node is ready to get it
	/// </summary>
	public static async Task<Node> WaitForNode(this Node self, string nodeName, int timeout = 5) => await self.WaitForNode<Node>(nodeName, timeout);

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region WaitUntilNodeReady

	/////// GENERIC ///////
	/// <summary>
	/// Wait until a node is ready but doesn't return it, for that use <see cref="WaitForNode"/>
	/// </summary>
	public static async Task WaitUntilNodeReady(this Node self, string nodeName, int timeout = 5)
	{
		int elapsed = 0;
		int to = timeout * 1000;
		Node node = self.GetNode(nodeName);
		
		while (!node.IsNodeReady()) {
			await Task.Delay(25);
			elapsed += 25;
			
			if (elapsed >= to) {
				throw new TimeoutException();
			}
		}
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region WaitUntilPropertyReady

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

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	#region Color.ToHex

	/// <summary>
	/// Returns the color's HTML hexadecimal color string in RGBA format.
	/// </summary>
	public static string ToHex(this Color self)
		=> self.ToHtml();

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region Lerp Functions

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

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region and wahtever this does

	public static Vector2 Snapped(this Vector2 vector, Vector2 gridSize)
	{
		return new Vector2(
			Mathf.Floor(vector.X / gridSize.X) * gridSize.X,
			Mathf.Floor(vector.Y / gridSize.Y) * gridSize.Y
		);
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region whatever this does

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

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region All

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

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
