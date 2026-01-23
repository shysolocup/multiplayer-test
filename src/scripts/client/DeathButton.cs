using Godot;

[Prerunner]
public partial class DeathButton : ClientBehavior
{
	public override void OnKeyPressed(InputEventKey key, bool unhandled)
	{
		if (key.Keycode == Key.Escape)
		{
			game.Quit();
		}
	}
}
