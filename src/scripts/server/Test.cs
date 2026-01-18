using Godot;

[OnServer]
public partial class Test : Behavior
{
    private CsgBox3D part { get; set; }

    public override void OnReady()
    {
        part = workspace.GetNode<CsgBox3D>("Part");

        part.Position = new Vector3(0, 100, 0);
    }
}