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

		server.StartedHosting += id =>
		{
			print(id);
			lobby.Text = id;
		};

		joinButton.Pressed += async () => 
		{
			print("got it boss, joining!!");
			await rep.Join(lobby.Text);
		};

		hostButton.Pressed += async () =>
		{
			print("got it boss, hosting!!");
			await rep.Host();
		};
	}
}
