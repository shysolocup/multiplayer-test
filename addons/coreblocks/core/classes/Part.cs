using Godot;


[Tool]
[GlobalClass, Icon("uid://cqfrrgjnbgcur")]
public partial class Part : CsgBox3D
{
    [Export]
    public Color Color
    {
        get => Material is StandardMaterial3D mat ? mat.AlbedoColor : Colors.White;
        set {
            if (Material is StandardMaterial3D mat)
            {
                mat.AlbedoColor = value;
            }
        }
    }

    [Export]
    public Texture2D Texture
    {
        get => Material is StandardMaterial3D mat ? mat.AlbedoTexture : null;
        set {
            if (Material is StandardMaterial3D mat)
            {
                mat.AlbedoTexture = value;
            }
        }
    }

    [Export]
    public bool Shaded
    {
        get => Material is StandardMaterial3D mat ? mat.ShadingMode != BaseMaterial3D.ShadingModeEnum.Unshaded : true;
        set {
            if (Material is StandardMaterial3D mat)
            {
                mat.ShadingMode = value ? BaseMaterial3D.ShadingModeEnum.PerPixel : BaseMaterial3D.ShadingModeEnum.Unshaded;
            }
        }
    }

    [Export]
    public bool CanCollide
    {
        get => UseCollision;
        set => UseCollision = value;
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        Material ??= new StandardMaterial3D
        {
            AlbedoColor = Color,
            AlbedoTexture = Texture,
            ShadingMode = Shaded ? BaseMaterial3D.ShadingModeEnum.PerPixel : BaseMaterial3D.ShadingModeEnum.Unshaded
        };
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}