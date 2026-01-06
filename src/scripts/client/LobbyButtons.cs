using Godot;

public partial class LobbyButtons : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		var container = gui.GetNode("./rect/container");

		var joinButton = container.GetNode<Button>("joinButton");
		var hostButton = container.GetNode<Button>("hostButton");
		var lobby = container.GetNode<LineEdit>("lobbyId");

		joinButton.Pressed += async () => 
		{
			print("got it boss, joining!!");
			await server.Join(lobby.Text);
		};

		hostButton.Pressed += async () =>
		{
			print("got it boss, hosting!!");
			await server.Host();
		};
	}
}
