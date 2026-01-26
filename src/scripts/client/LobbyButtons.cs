using Godot;

[Prerunner]
public partial class LobbyButtons : ClientBehavior
{
	private LineEdit lobby;
	private Button hostButton;
	private Button joinButton;


	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		GD.Print("guh lobby buttons script is running");

		var container = gui.GetNode("./lobby/container");

		joinButton = container.GetNode<Button>("joinButton");
		hostButton = container.GetNode<Button>("hostButton");
		lobby = container.GetNode<LineEdit>("lobbyId");

		joinButton.Pressed += join;
		hostButton.Pressed += host;

		game.StartedHosting += hosting;
		game.Joining += joining;

		mouse.BindActor(
			actor: this, 
			channel: Enum.PriorityChannel.Ui, 
			mode: Input.MouseModeEnum.Visible,
			persist: true
		);
	}


	private void joining(string id)
	{
		mouse.UnbindActor(this, Enum.PriorityChannel.Ui);
		goaway();
	}

	private void hosting(string id)
	{
		DisplayServer.ClipboardSet(id);
		lobby.Text = id;
		goaway();

		mouse.UnbindActor(this, Enum.PriorityChannel.Ui);
	}

	private async void join()
	{
		if (string.IsNullOrEmpty(lobby.Text)) return;

		print("got it boss, joining!!");

		joinButton.Disabled = true;
		hostButton.Disabled = true;

		await game.Join(lobby.Text);
	}

	private async void host()
	{
		print("got it boss, hosting!!");

		joinButton.Disabled = true;
		hostButton.Disabled = true;

		await game.Host();
	}

	private void goaway()
	{
		foreach (var node in GetTree().GetNodesInGroup("lobbyui"))
		{
			if (node is Control control)
			{
				control.Hide();
			}
		}
	}
}
