using Godot;
using System;

public partial class JoinButton : Behavior
{
	// Called when the script node and its dependencies are ready.
	public override async void OnReady()
	{
		warn("Hello World!");

		warn(workspace);

		var button = GetParent<Button>();

		button.Pressed += async () =>
		{
			var server = await Server.Instance();

			GD.PushWarning("loaded client stuff");

			await server.Join(server.HostId);
		};
	}
}
