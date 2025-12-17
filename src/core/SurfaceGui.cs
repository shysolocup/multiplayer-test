using System;
using Godot;

[Tool]
[GlobalClass, Icon("uid://c31ypj3dcauw3")]
public partial class SurfaceGui : Sprite3D
{
    [Export]
    public VisualInstance3D Adornee;

    public enum Surfaces
    {
        Front = 0,
        Back = 1,
        Left = 2,
        Right = 3,
        Top = 4,
        Bottom = 5
    }

    [Export]
    public Surfaces Face = Surfaces.Front;

    [Export]
    public float FloatingOffset = 0.001f;

    [Export]
    public Vector3 PositionOffset = new();

    [Export]
    public Vector3 ScaleOffset = new();

    private SubViewport viewport;


    public SubViewport Viewport
    {
        get => viewport;
        set
        {
            if (value is not null && Texture is ViewportTexture texture) texture.ViewportPath = value.GetPath();
            viewport = value;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        if (!Engine.IsEditorHint())
        {
            Adornee ??= GetParent<VisualInstance3D>();
        }

        Texture ??= new ViewportTexture();

        ChildOrderChanged += () => Viewport = GetChildOrNull<SubViewport>(0);
        Viewport = GetChildOrNull<SubViewport>(0);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Adornee is not null && Texture is not null && Viewport is not null)
        {
            var global = Adornee.GlobalTransform; // basically just a cframe
            var origin = global.Origin;
            var basis = global.Basis; // just the rotation of the cframe

            var forward = basis.Z;
			var left = basis.X;
			var up = basis.Y;

            var meteredAdornee = Adornee.GetAabb().Size;
            var meteredViewport = Viewport.Size * new Vector2(PixelSize, PixelSize);

            Vector3 nforward;

            var offset = FloatingOffset + 1;
            Vector3 direction = forward;

            switch (Face)
            {
                case Surfaces.Front:
                    direction = -forward;
                    forward = -forward;
                    left = -left;
                    break;

                case Surfaces.Left:
                    direction = -left;
                    nforward = -left;
                    left = forward;
                    forward = nforward;
                    break;

                case Surfaces.Right:
                    direction = left;
                    nforward = left;
                    left = -forward;
                    forward = nforward;
                    break;

                case Surfaces.Top:
                    direction = up;
                    nforward = up;
                    up = -forward;
                    forward = nforward;
                    break;

                case Surfaces.Bottom:
                    direction = -up;
                    nforward = -up;
                    up = forward;
                    forward = nforward;
                    break;
            }

            origin += direction / 2 * offset;
            origin += PositionOffset;

            GlobalTransform = new(left, up, forward, origin);
            Scale = new Vector3(meteredAdornee.X / meteredViewport.X, meteredAdornee.Y / meteredViewport.Y, 1) + ScaleOffset;
        }
    }
}