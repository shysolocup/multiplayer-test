using System.Linq;
using Godot;

[Prerunner]
public partial class DeathButton : Behavior
{
	public override void OnKeyPressed(InputEventKey key)
	{
		if (key.Keycode == Key.Escape)
		{
			game.Quit();
		}
	}
}
