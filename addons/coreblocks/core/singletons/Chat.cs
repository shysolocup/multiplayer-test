using System;
using Godot;
using Godot.Collections;


[GlobalClass, Icon("uid://crh2p5n6n6h2")]
public partial class Chat : SingletonControl<Chat>
{
    private static MultiplayerSpawner ChannelSpawner { get; set; }
    public static LineEdit Label { get; set; }
    public static Button SendButton { get; set; }
    public static TabContainer Channels { get; set; }
    public static Timer DisappearTimer { get; set; }

    private CoreGui coregui { get; set; }

    [Export]
    public PackedScene StarterChannel = GD.Load<PackedScene>("res://addons/coreblocks/scenes/starter_channel.tscn");

    [Export]
    public Theme MessageTheme = GD.Load<Theme>("res://addons/coreblocks/assets/materials/coregui_message_style.tres");

    public enum TextChannel
    {
        General = 0,
        Team = 1
    }

    public override async void _Ready()
    {
        base._Ready();

        await Game.WaitUntilConnected();

        DisappearTimer = GetNode<Timer>("./disappear");

        Channels = GetNode<TabContainer>("./channels");
        ChannelSpawner = GetNode<MultiplayerSpawner>("./channelSpawner");
        ChannelSpawner.SpawnFunction = tabCall;
        
        var container = GetNode("./inputContainer");
        Label = container.GetNode<LineEdit>("./input");
        SendButton = container.GetNode<Button>("./send"); 

        coregui = await CoreGui.Instance();

        DisappearTimer.Timeout += Hide;
        DisappearTimer.Start();

        if (Game.IsServer())
        {
            MakeChannel("General");
            MakeChannel("Team");
        }

        SendButton.Pressed += sendMessage;
        Label.TextSubmitted += text => sendMessage();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventKey key && key.Keycode == Key.Slash)
        {
            Visible = true;
            DisappearTimer.Restart();
        } 
    }

    private ScrollContainer makeTab(string tabName)
    {
        var tab = StarterChannel.Instantiate<ScrollContainer>();
        tab.Name = tabName;

        var spawner = tab.GetNode<MultiplayerSpawner>("messageSpawner");
        spawner.SpawnFunction = msgCall;

        var helper = tab.GetNode<RichTextLabel>("./content/helper");
        helper.Theme = MessageTheme;

        return tab;
    }

    private Callable tabCall => new(this, MethodName.makeTab);


    private RichTextLabel makeMessage(Godot.Collections.Array ctx)
    {
        var name = ctx[0].AsString();
        var msg = ctx[1].AsString();
        var color = ctx[0].AsString();

        return new RichTextLabel()
        {
            BbcodeEnabled = true,
            Text = $"[color=#{color}]{name}[/color]: {msg}",
            Theme = MessageTheme
        };
    }


    private Callable msgCall => new(this, MethodName.makeMessage);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public RichTextLabel MakeMessage(int index, string name, string message, int peerId)
    {
        var channel = GetChannel(index);
        message = message.Trim();
        var color = RandomNameColor((ulong)peerId);

        Behavior.assert(channel is not null, "Channel is null");
        if (string.IsNullOrEmpty(message)) return null;

        var spawner = channel.GetNode<MultiplayerSpawner>("messageSpawner");
        return (RichTextLabel)spawner.Spawn(new Godot.Collections.Array() { 
            name, message, color
        });
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void sendMessage()
        => SendMessage(Channels.CurrentTab, Label.Text);


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public Error SendMessage(int channel, string message)
        => Client.LocalPlayer is Player player ? RpcId(1, MethodName.MakeMessage, 
            
            channel, 
            message.Trim(), 
            player.GetPlayerName(),
            player.GetPeerId()

        ) : Error.Failed;


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public ScrollContainer MakeChannel(string channelName)
        => (ScrollContainer)ChannelSpawner.Spawn(channelName);


    public void Toggle()
    {
        Visible ^= true;

        if (Visible)
            DisappearTimer.Restart();
    }

    public ScrollContainer GetChannel(int tab)
        => Channels.GetChild<ScrollContainer>(tab);

    public ScrollContainer GetChannel(string tab)
        => Channels.GetNode<ScrollContainer>(tab);


    private static readonly RandomNumberGenerator random = new();

    public Color RandomNameColor(ulong seed)
    {
        random.Seed = seed;
    
        var r = random.Randf();
        var g = random.Randf();
        var b = random.Randf();
        
        return new Color(r, g, b);
    }
}