using System.Linq;
using Godot;

public partial class LobbyButtons : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		var container = gui.GetNode("./lobby/container");

		var joinButton = container.GetNode<Button>("joinButton");
		var hostButton = container.GetNode<Button>("hostButton");
		var lobby = container.GetNode<LineEdit>("lobbyId");

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

		rep.StartedHosting += id =>
		{
			DisplayServer.ClipboardSet(id);
			lobby.Text = id;
		};

		rep.NewConnection += (id) =>
		{
			print(id);
			goaway();
		};
	}

	private void goaway()
	{
		foreach (var node in GetTree().GetNodesInGroup("lobbyui"))
		{
			if (node is Control control)
			{
				control.Visible = false;
			}
		}
	}
}
