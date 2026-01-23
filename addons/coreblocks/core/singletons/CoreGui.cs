using Godot;


[GlobalClass]
public partial class CoreGui : SingletonCanvas<CoreGui>
{
    [Signal] public delegate void ShiftlockGuiEnabledEventHandler();
    [Signal] public delegate void ShiftlockGuiDisabledEventHandler();

    [Signal] public delegate void PauseGuiEnabledEventHandler();
    [Signal] public delegate void PauseGuiDisabledEventHandler();

    private bool shiftlockGui = true;
    private bool pauseGui = true;

    [Export] public bool DefaultShiftlockGuiEnabled
    {
        get => shiftlockGui;
        set
        {
            if (value && !shiftlockGui) {
                EmitSignalShiftlockGuiEnabled();
            }
            else if (!value && shiftlockGui)
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

    public static HBoxContainer TopBar { get; set; }

    public static Button PauseButton { get; set; }
    public static Button ChatButton { get; set; }
    public static Chat Chat { get; set; }
    public static TextureRect Crosshair { get; set; }

    public override async void _Ready()
    {
        base._Ready();

        var cameras = await CameraSystem.Instance();

        TopBar = GetNode<HBoxContainer>("./topbar");

        PauseButton = TopBar.GetNode<Button>("./pause");
        ChatButton = TopBar.GetNode<Button>("./chat");

        Crosshair = GetNode<TextureRect>("./crosshair");

		Crosshair.Hide();

		if (DefaultShiftlockGuiEnabled)
		{
			cameras.ShiftLockEnabled += Crosshair.Show;
			cameras.ShiftLockDisabled += Crosshair.Hide;	
		}

        Chat = await Chat.Instance();

        ChatButton.Pressed += Chat.Toggle;
        PauseButton.Pressed += TogglePause;
    }

    public void TogglePause()
    {
        
    }

}