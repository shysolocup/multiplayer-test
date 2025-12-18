using Godot;
using Godot.Collections;
using System.Text.RegularExpressions;

[Tool]
[GlobalClass, Icon("uid://dooue5vjaj4k")]
public partial class Trigger3D : Area3D
{
	[Signal] public delegate void TouchedEventHandler(Node node);
	[Signal] public delegate void TouchEndedEventHandler(Node node);

	[Export] public Array<Node> TouchingNodes = [];

	[Export] public bool Triggered = false;
	[Export] public bool Enabled = true;

	[Export] public Array<string> Filter = [];
	[Export] public Array<string> TypeFilter = [];
	[Export] public Array<string> DescendantFilter = [];

	[Export(PropertyHint.Enum, "Exclude:1,Include:2")] public int FilterType = 1;

	public override void _Ready()
	{
		Monitoring = true;
		Monitorable = true;

		base._Ready();

		BodyEntered += TouchedHandle;
		BodyExited += TouchEndedHandle;

		Touched += _Touched;
		TouchEnded += _TouchEnded;
	}

	public virtual void _Touched(Node diddler) {}
	public virtual void _TouchEnded(Node diddler) {}

	private void TouchedHandle(Node diddler)
	{
		if (!EventHandler(diddler)) EmitSignalTouched(diddler);
	}

	private void TouchEndedHandle(Node diddler)
	{
		if (!EventHandler(diddler)) EmitSignalTouchEnded(diddler);
	}

	private bool EventHandler(Node diddler)
	{
		bool flagged = false;

		foreach (string filter in Filter) {
			switch(FilterType) {
				case 1: { // Exclude
					flagged = flagged || Regex.IsMatch(diddler.Name, filter);
					break;
				}
				case 2: { // Include
					flagged = flagged || !Regex.IsMatch(diddler.Name, filter);
					break;
				}
			}
		}

		foreach (string typeFilter in TypeFilter) {
			switch(FilterType) {
				case 1: { // Exclude
					flagged = flagged || Regex.IsMatch(diddler.GetType().Name, typeFilter);
					break;
				}
				case 2: { // Include
					flagged = flagged || !Regex.IsMatch(diddler.GetType().Name, typeFilter);
					break;
				}
			}
		}

		return flagged;
	}
}
