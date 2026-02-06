using Godot;


[NotReplicated]
[GlobalClass]
public partial class CoreGui : SingletonCanvas<CoreGui>
{
    [Signal] public delegate void ShiftlockGuiEnabledEventHandler();
    [Signal] public delegate void ShiftlockGuiDisabledEventHandler();

    [Signal] public delegate void PauseGuiEnabledEventHandler();
    [Signal] public delegate void PauseGuiDisabledEventHandler();

    [Signal] public delegate void ChatGuiEnabledEventHandler();
    [Signal] public delegate void ChatGuiDisabledEventHandler();

    private bool shiftlockGui = true;
    private bool pauseGui = true;
    private bool chatGui = true;

    [Export] public bool DefaultShiftlockGuiEnabled
    {
        get => shiftlockGui;
        set
        {
            if (value && !shiftlockGui) {
                Connect(SignalName.ShiftlockGuiEnabled, showShiftlock);
                Connect(SignalName.ShiftlockGuiDisabled, hideShiftlock);

                EmitSignalShiftlockGuiEnabled();
            }
            else if (!value && shiftlockGui)
                Disconnect(SignalName.ShiftlockGuiEnabled, showShiftlock);
                Disconnect(SignalName.ShiftlockGuiDisabled, hideShiftlock);

                EmitSignalShiftlockGuiDisabled();

            shiftlockGui = value;
        }
    }
    [Export] public bool DefaultPauseGuiEnabled {
        get => pauseGui;
        set
        {
            if (value && !pauseGui) {
                EmitSignalPauseGuiEnabled();

            }
            else if (!value && pauseGui)
                EmitSignalPauseGuiDisabled();

            pauseGui = value;
        }
    }
    [Export] public bool DefaultChatGuiEnabled {
        get => chatGui;
        set
        {
            if (value && !chatGui) {
                Connect(SignalName.ChatGuiEnabled, showChat);
                Connect(SignalName.ChatGuiDisabled, hideChat);

                EmitSignalChatGuiEnabled();

            }
            else if (!value && chatGui)
                Disconnect(SignalName.ChatGuiEnabled, showChat);
                Disconnect(SignalName.ChatGuiDisabled, hideChat);

                EmitSignalChatGuiDisabled();

            pauseGui = value;
        }
    }

    public static HBoxContainer TopBar { get; set; }

    public static Button PauseButton { get; set; }
    public static Button ChatButton { get; set; }
    public static Chat Chat { get; set; }
    public static TextureRect Crosshair { get; set; }


    private Callable showShiftlock => new(Crosshair, MethodName.Show);
    private Callable hideShiftlock => new(Crosshair, MethodName.Hide);
    private Callable showChat => new(this, MethodName.EnableChat);
    private Callable hideChat => new(this, MethodName.DisableChat);


    public override async void _Ready()
    {
        Hide();

        base._Ready();

        var cameras = await CameraSystem.Instance();

        TopBar = GetNode<HBoxContainer>("./topbar");

        PauseButton = TopBar.GetNode<Button>("./pause");
        ChatButton = TopBar.GetNode<Button>("./chat");

        Crosshair = GetNode<TextureRect>("./crosshair");

		Crosshair.Hide();

        Chat = await Chat.Instance();

        ChatButton.Pressed += Chat.Toggle;
        PauseButton.Pressed += TogglePause;

        if (DefaultShiftlockGuiEnabled)
		{
            Connect(SignalName.ShiftlockGuiEnabled, showShiftlock);
            Connect(SignalName.ShiftlockGuiDisabled, hideShiftlock);
		}

        if (DefaultChatGuiEnabled)
        {
            Connect(SignalName.ChatGuiEnabled, showChat);
            Connect(SignalName.ChatGuiDisabled, hideChat);
        }

        await Game.WaitUntilConnected();

        Show();
    }

    public void DisableChat()
    {
        Chat.Hide();
        ChatButton.Hide();
    }

    public void EnableChat()
    {
        Chat.Hide();
        ChatButton.Hide();
    }

    public void TogglePause()
    {
        
    }

}