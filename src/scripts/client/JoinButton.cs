using Godot;
using System;

public partial class JoinButton : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		var container = gui.GetNode("./rect/container");

		var button = container.GetNode<Button>("joinButton");
		var lobby = container.GetNode<LineEdit>("lobbyId");

		button.Pressed += async () => {
			print("got it boss");
			await server.Join(lobby.Text);
		};
	}
}
