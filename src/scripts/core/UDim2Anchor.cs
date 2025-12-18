using Godot;

// DisplayServer.WindowGetSize();

[Tool]
[GlobalClass, Icon("uid://cb7frf5mruesy")]
public partial class UDim2Anchor : Node
{

	[ExportGroup("Operations")]

	[Export]
    public Control Adornee;

	[Export(PropertyHint.Enum, "Scale:0,Offset:1")]
	public int ResizeMode = 0;

	[Export]
	public bool Canvas = false;
	[Export]
	public bool AnchorSize = false;
	[Export]
	public bool AnchorPosition = false;
	[Export]
	public bool AnchorPivot = false;


	[ExportGroup("Dynamics")]

	[Export]
	public UDim2 Size = UDim2.DefaultSize;
	[Export]
	public UDim2 Position = UDim2.Zero;
	[Export]
	public Vector2 AnchorPoint = new(0.5f, 0.5f);
	[Export]
	public Vector2 PivotAnchor = new(0.5f, 0.5f);

	public override void _Ready()
	{
		base._Ready();

		if (!Engine.IsEditorHint())
		{
			Adornee ??= GetParent<Control>();
		}
	}

	public void guh<T>() where T : Control
	{

	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Update();
    }

	public void Update()
	{
		if (Adornee is Control c)
		{
			Vector2 basis = Canvas ? DisplayServer.WindowGetSize() : c.GetParentAreaSize(); // probably works

			if (AnchorSize)
			{
				c.Size = Size.ToVector2(basis);
			}

			if (AnchorPosition)
			{
				c.Position = Position.ToVector2(basis) - (AnchorPoint * Size.ToVector2(basis));
			}

			if (AnchorPivot) {
				c.PivotOffset = c.Size * PivotAnchor;
			}
		}
	}
}
