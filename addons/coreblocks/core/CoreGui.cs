using Godot;

[GlobalClass]
public partial class CoreGui : SingletonCanvas<CoreGui>
{
    [Export] public bool DefaultShiftlockGuiEnabled = true;
    [Export] public bool DefaultPauseGuiEnabled = true;
}