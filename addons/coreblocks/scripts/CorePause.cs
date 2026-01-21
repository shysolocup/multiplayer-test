using Godot;


[OnClient]
public partial class CorePause : Behavior
{
    private CoreGui coreGui;
    private Button button;

    public override async void OnReady()
    {
        coreGui = await CoreGui.Instance();
        button = coreGui.GetNode<Button>("./topbar/pause");
    }
}