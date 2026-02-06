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

    protected private static MultiplayerSpawner ChannelSpawner { get; set; }
    public static LineEdit Label { get; set; }
    public static Button SendButton { get; set; }
    public static TabContainer Channels { get; set; }
    public static Timer DisappearTimer { get; set; }

    private CoreGui coregui { get; set; }
    private Replicator rep { get; set; }
    private Players players { get; set; }
    private Server server { get; set; }

    [Export]
    public PackedScene StarterChannel = GD.Load<PackedScene>("res://addons/coreblocks/scenes/starter_channel.tscn");

    [Export]
    public Theme MessageTheme = GD.Load<Theme>("res://addons/coreblocks/assets/materials/coregui_message_style.tres");


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
        rep = await Replicator.Instance();
        players = await Players.Instance();
        server = await Server.Instance();

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

        GotoChannel(Enum.TextChannel.General);

        SendButton.Pressed += sendMessage;
        Label.TextSubmitted += text => sendMessage();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventKey key && key.Keycode == Key.Slash)
        {
            if (!Game.IsConnected()) return;
            
            Visible = true;
            Label.GrabFocus();
            DisappearTimer.Start();
        } 
    }

    protected private ScrollContainer makeTab(string tabName)
    {
        var tab = StarterChannel.Instantiate<ScrollContainer>();
        tab.Name = tabName;

        return tab;
    }

    protected private Callable tabCall => new(this, MethodName.makeTab);


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void EmitMessage(ulong id, string message, int index)
    {
        var player = (Player)InstanceFromId(id);
        EmitSignalNewMessage(player, message, index);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    protected private RichTextLabel MakeMessage(int index, string message, string name, ulong id)
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

        DisappearTimer.Start();

        return label;
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void sendMessage()
    {
        var id = (int)Client.LocalPlayer.GetPeerId();
        var text = Label.Text;
        var tab = Channels.CurrentTab;

        if (Game.IsServer())
            SendMessage(tab, text, id);
        else
            server.Invoke(
                this, 
                MethodName.SendMessage, 
                tab,
                text,
                id
            );
        
        Label.Text = "";
        Label.ReleaseFocus();
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public Error SendMessage(int channel, string message, int id)
    {
        var player = players.GetPlayerById(id);

        // if it is the server then send the message out to all clients
        if (Game.IsServer())
        {
            return Rpc(
                MethodName.MakeMessage, 
                channel, 
                message.Trim(), 
                player.GetPlayerName(),
                player.GetInstanceId()
            );
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

        rep.StartReplicating(channel, true);

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

    public void GotoChannel(Enum.TextChannel channel) 
        => GotoChannel((int)channel);

    public void GotoChannel(string channel)
        => Channels.CurrentTab = GetChannel(channel).GetIndex();

    public ScrollContainer GetChannel(int channel)
        => Channels.GetChildOrNull<ScrollContainer>(channel);

    public ScrollContainer GetChannel(Enum.TextChannel channel)
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