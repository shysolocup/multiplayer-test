using System.Linq;
using Godot;


public partial class LobbyButtons : Behavior
{
	private LineEdit lobby;
	private Button hostButton;
	private Button joinButton;


	// Called when the script node and its dependencies are ready.
	public override void OnReady()
	{
		var container = gui.GetNode("./lobby/container");

		mouse.BindActor(
			actor: this, 
			priority: Mouse.PriorityChannel.Ui, 
			mode: Input.MouseModeEnum.Visible,
			persist: true
		);

		joinButton = container.GetNode<Button>("joinButton");
		hostButton = container.GetNode<Button>("hostButton");
		lobby = container.GetNode<LineEdit>("lobbyId");

		joinButton.Pressed += join;
		hostButton.Pressed += host;
		rep.StartedHosting += hosting;
		rep.Joining += joining;
	}


	private void joining(string id)
	{
		mouse.UnbindActor(this, Mouse.PriorityChannel.Ui);
		goaway();
	}

	private void hosting(string id)
	{
		DisplayServer.ClipboardSet(id);
		lobby.Text = id;
		goaway();

		mouse.UnbindActor(this, Mouse.PriorityChannel.Ui);
	}

	private async void join()
	{
		print("got it boss, joining!!");
		await rep.Join(lobby.Text);
	}

	private async void host()
	{
		print("got it boss, hosting!!");
		await rep.Host();
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
