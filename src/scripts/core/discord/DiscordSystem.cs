using Godot;
using System;

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordSystem : Node
{
	static public Node DiscordRPCNode;
	// private PlaceController pc;

	public DiscordSystem SetStatus(DiscordStatus status)
	{
		bool hasSmallImage = status.SmallImage != DiscordStatusImage.none;

		DiscordRPCNode.Set("Details", status.Details ?? default);
		DiscordRPCNode.Set("State", status.State.Trim() == "" /*? place.Epoch.Title : status.State*/);
		DiscordRPCNode.Set("LargeImage", status.LargeImage.ToString() ?? default);
		DiscordRPCNode.Set("LargeImageText", status.LargeImageText ?? default);
		DiscordRPCNode.Set("SmallImage", hasSmallImage ? status.LargeImage.ToString() ?? default : "");
		DiscordRPCNode.Set("SmallImageText", hasSmallImage ? status.SmallImageText ?? default : "");
		
		return this;
	}

	public DiscordSystem RefreshStatus()
	{
		SetStatus(DiscordStatus.Default).RefreshRPC();
		return this;
	}

	public override void _Ready()
	{
		base._Ready();

		GDScript rpcscript = GD.Load<GDScript>("res://src/core/discord/DiscordRPCNode.gd");
		DiscordRPCNode = (Node)rpcscript.New();

		/*pc = GetNode<PlaceController>("../%PlaceController");

		await ToSignal(pc, Node.SignalName.Ready);

		pc.PlaceChanged += () => {
			RefreshStatus();
		};*/

		RefreshStatus();
		GD.Print(IsWorking());
	}

	public bool IsWorking() 
	{
		return (bool)DiscordRPCNode.Call("IsWorking");
	}

	public void RefreshRPC()
	{
		DiscordRPCNode.Call("RefreshRPC");
	}

	public void NewTimestamp()
	{
		DiscordRPCNode.Call("NewTimestamp");
	}
}
