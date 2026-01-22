using Godot;

[Prerunner]
public partial class DeathButton : ClientBehavior
{
	public override void OnKeyPressed(InputEventKey key)
	{
		if (key.Keycode == Key.Escape)
		{
			game.Quit();
		}
	}
}
