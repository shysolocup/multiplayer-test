using Godot;

[Tool]
[GlobalClass]
[NotReplicated]
public partial class Screen : Singleton<Screen>
{
    public static Vector2I WindowSize {
        get => DisplayServer.WindowGetSize();
        set => DisplayServer.WindowSetSize(value);
    }
    public static Vector2I ScreenSize => DisplayServer.ScreenGetSize();
    public static int CurrentScreen => DisplayServer.GetPrimaryScreen();

    public Error Popup(string title, string description, string[] buttons, Callable callback) => DisplayServer.DialogShow(title, description, buttons, callback); 

    private bool fullscreen = false;

    public bool Fullscreen
    {
        get => fullscreen;
        set {
            fullscreen = value;

            #if TOOLS
                // do nothing here
                // if it's running in the editor it shouldn't try to fullscreen
            #else
            DisplayServer.WindowSetMode(
                value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed
            );
            #endif
        }
    }

    public override void _Ready()
    {
        base._Ready();

        #if TOOLS
            // do nothing here
            // if it's running in the editor it shouldn't try to fullscreen
        #else
    
        DisplayServer.WindowSetMode(
            value ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed
        );
        
        #endif
    }
}