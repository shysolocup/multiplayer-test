using System.Linq;
using Godot;


public partial class DeathButton : Behavior
{
	public override void OnKeyPressed(InputEventKey key)
	{
		if (key.Keycode == Key.Escape)
		{
			GetTree().Quit();
		}
	}
}
