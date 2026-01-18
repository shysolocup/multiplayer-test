using Godot;

[OnServer]
public partial class Test : Behavior
{
    private CsgBox3D part { get; set; }

    public override void OnReady()
    {
        print("Test script is ready on the server!");

        part = workspace.GetNode<CsgBox3D>("Part");
    }
}