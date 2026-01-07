using Godot;

[GlobalClass, Icon("uid://bwcxp8wfwv51d")]
public partial class UDim2 : Resource
{

	[Export] public Vector2 X = Vector2.Zero;
	[Export] public Vector2 Y = Vector2.Zero;

	private Vector2 defaultAnchor = new(0.5f, 0.5f);
	private Vector2 defaultPivot = new(0.5f, 0.5f);


	public Vector2 ToVectorSpace( Vector2 anchorPoint, Vector2 pivotPoint, Control inside = null)
	{
		Vector2 basis = inside is not null ? inside.GetParentAreaSize() : DisplayServer.WindowGetSize();

		Vector2 size = ToVector2(basis);

		Vector2 anchorOffset = anchorPoint * basis;
		Vector2 pivotOffset  = size * pivotPoint;

		return size - anchorOffset - pivotOffset;
	}


	public Vector2 Scale
	{
		get => new(X.X, Y.X);
	}

	public Vector2 Offset
	{
		get => new(X.Y, Y.Y);
	}


	static public UDim2 Zero { get => new(0, 0, 0, 0); }
	static public UDim2 DefaultSize { get => new(0, 200, 0, 200); }


	public UDim2 Lerp(UDim2 to, float weight) => 
		new(X.Lerp(to.X, weight), Y.Lerp(to.Y, weight));

	public Vector2 ToVector2(Vector2 AdorneeSize) => 
		new((Scale.X * AdorneeSize.X) + Offset.X, (Scale.Y * AdorneeSize.Y) + Offset.Y);


	public UDim2() : base() { }

	public UDim2(float xScale, float xOffset, float yScale, float yOffset) : base()
	{
		X = new(xScale, xOffset);
		Y = new(yScale, yOffset);
	}

	public UDim2(Vector2 X, Vector2 Y) : base() 
	{
		this.X = X;
		this.Y = Y;
	}

	static public UDim2 FromScale(float xScale, float yScale) => new(xScale, 0, yScale, 0);
	static public UDim2 FromOffset(float xOffset, float yOffset) => new(0, xOffset, 0, yOffset);


	public static UDim2 operator + (UDim2 a, dynamic b) => new(a.X + b.X, a.Y + b.Y);
	public static UDim2 operator - (UDim2 a, dynamic b) => new(a.X - b.X, a.Y - b.Y);
	public static UDim2 operator * (UDim2 a, dynamic b) => new(a.X * b.X, a.Y * b.Y);
	public static UDim2 operator / (UDim2 a, dynamic b) => new(a.X / b.X, a.Y / b.Y);
	public static UDim2 operator ^ (UDim2 a, dynamic b) => new(a.X ^ b.X, a.Y ^ b.Y);
	public static UDim2 operator % (UDim2 a, dynamic b) => new(a.X % b.X, a.Y % b.Y);
	public static bool operator == (UDim2 a, dynamic b) => a.X == b.X && a.Y == b.Y;
	public static bool operator != (UDim2 a, dynamic b) => a.X != b.X && a.Y != b.Y;


	public override bool Equals(object obj)
	{
		if (ReferenceEquals(this, obj)) return true;
		if (obj is UDim2 other) return this == other;
		return false;
	}

	public override int GetHashCode() => throw new System.NotImplementedException();
}
