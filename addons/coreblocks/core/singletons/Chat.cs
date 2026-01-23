using System;
using Godot;
using Godot.Collections;


[GlobalClass, Icon("uid://crh2p5n6n6h2")]
public partial class Chat : SingletonControl<Chat>
{
    [Signal]
    public delegate void NewMessageEventHandler(Player player, string message, int channel);

    [Signal]
    public delegate void NewChannelEventHandler(int channel);

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


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public override async void _Ready()
    {
        base._Ready();

        NewMessage += (sender, msg, channel) => GD.Print(sender, msg);
        NewChannel += (channel) => GD.Print(channel);

        ChannelSpawner = GetNode<MultiplayerSpawner>("./channelSpawner");
        ChannelSpawner.SpawnFunction = tabCall;

        Hide();

        await Game.WaitUntilConnected();
        coregui = await CoreGui.Instance();

        Show();

        DisappearTimer = GetNode<Timer>("./disappear");

        Channels = GetNode<TabContainer>("./channels");
        
        var container = GetNode("./inputContainer");
        Label = container.GetNode<LineEdit>("./input");
        SendButton = container.GetNode<Button>("./send"); 

        DisappearTimer.Timeout += Hide;
        DisappearTimer.Start();

        if (Game.IsServer())
        {
            MakeChannel("General");
            MakeChannel("Team");
        }

        GotoChannel(TextChannel.General);

        SendButton.Pressed += sendMessage;
        Label.TextSubmitted += text => sendMessage();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventKey key && key.Keycode == Key.Slash)
        {
            Visible = true;
            Label.GrabFocus();
            DisappearTimer.Start();
        } 
    }

    private ScrollContainer makeTab(string tabName)
    {
        var tab = StarterChannel.Instantiate<ScrollContainer>();
        tab.Name = tabName;

        return tab;
    }

    private Callable tabCall => new(this, MethodName.makeTab);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void EmitMessage(ulong id, string message, int index)
    {
        var player = (Player)InstanceFromId(id);
        EmitSignalNewMessage(player, message, index);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public RichTextLabel MakeMessage(int index, string message, string name, ulong id)
    {
        if (Game.IsServer())
        {
            var channel = GetChannel(index);
            message = message.Trim();

            Behavior.assert(channel is not null, "Channel is null");
            if (string.IsNullOrEmpty(message)) return null;

            var player = (Player)InstanceFromId(id);

            var color = PlayerColor(id).ToHtml();

            var label = new RichTextLabel()
            {
                BbcodeEnabled = true,
                Text = $"[color=#{color}]{name}[/color]: {message}",
                FitContent = true,
                Theme = MessageTheme
            };

            channel.GetNode("./content").AddChild(label);

            Rpc(MethodName.EmitMessage, player.GetInstanceId(), message, index);

            return label;   
        }

        return null;
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void sendMessage()
    {
        SendMessage(Channels.CurrentTab, Label.Text);
        Label.Text = "";
        Label.ReleaseFocus();
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public Error SendMessage(int channel, string message)
    {
        if (Client.LocalPlayer is Player player)
        {
            if (Game.IsServer())
            {
                MakeMessage(
                    channel, 
                    message.Trim(), 
                    player.GetPlayerName(),
                    player.GetInstanceId()
                );

                return Error.Ok;
            }
            else
            {
                return RpcId(1, MethodName.MakeMessage, 
                    channel, 
                    message.Trim(), 
                    player.GetPlayerName(),
                    player.GetInstanceId()
                );
            }
        }

        return Error.Failed;
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void EmitChannel(int channelId)
    {
        EmitSignalNewChannel(channelId);
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public ScrollContainer MakeChannel(string channelName)
    {
        var channel = (ScrollContainer)ChannelSpawner.Spawn(channelName);

        var helper = channel.GetNode<RichTextLabel>("./content/helper");
        helper.Theme = MessageTheme;

        Rpc(MethodName.EmitChannel, channel.GetIndex());

        return channel;
    }


    public void Toggle()
    {
        Visible ^= true;

        if (Visible) 
            DisappearTimer.Start();
    }

    public ScrollContainer GetCurrentChannel()
        => GetChannel(Channels.CurrentTab);

    public void GotoChannel(int channel)
        => Channels.CurrentTab = channel;

    public void GotoChannel(TextChannel channel) 
        => GotoChannel((int)channel);

    public void GotoChannel(string channel)
        => Channels.CurrentTab = GetChannel(channel).GetIndex();

    public ScrollContainer GetChannel(int channel)
        => Channels.GetChildOrNull<ScrollContainer>(channel);

    public ScrollContainer GetChannel(TextChannel channel)
        => GetChannel((int)channel);

    public ScrollContainer GetChannel(string tab)
        => Channels.GetNode<ScrollContainer>(tab);


    private static readonly RandomNumberGenerator random = new();

    public Color PlayerColor(ulong seed)
    {
        random.Seed = seed;
    
        var r = random.Randf();
        var g = random.Randf();
        var b = random.Randf();
        
        return new Color(r, g, b);
    }
}