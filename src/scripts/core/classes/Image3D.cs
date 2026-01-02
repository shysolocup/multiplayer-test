using Godot;

[Tool]
[GlobalClass, Icon("uid://ck61ilinbabbn")]
public partial class Image3D : MeshInstance3D
{
    private StandardMaterial3D mat = new();
    private Texture2D source;
    private float imageScale = 1;
    private float transparency = 0;

    [Export] public Texture2D Source {
        get => source;
        set {
            if (source == value) return;
            source = value;
            Changed();
        }
    }

    /// <summary>
    /// if true then the image will disappear at runtime
    /// </summary>
    [Export] public bool Reference = false;

    [Export(PropertyHint.Range, "0,2,or_greater,or_less")] public float ImageScale {
        get => imageScale;
        set {
            if (imageScale == value) return;
            imageScale = value;
            Scale = Rescale();
        }
    }
    
    [Export(PropertyHint.Range, "0,1,")]
    public float Opacity
    {
        get => transparency;
        set
        {
            if (transparency == value) return;
            transparency = value;
            Changed();
        }
    }

    public Vector3 Rescale()
    {
        Vector2 scale = source.GetSize() / 500 * imageScale;
        return new Vector3(scale.X, 1, scale.Y);
    }

    public void Changed()
    {
        if (source is not null) {
            Scale = Rescale();
        }

        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        Transparency = Opacity;

        CastShadow = ShadowCastingSetting.Off;

        mat.AlbedoTexture = source;
    }

    public override void _Ready()
    {
        base._Ready();

        if (Reference && !Engine.IsEditorHint()) Hide();

        Mesh ??= new PlaneMesh();
        Mesh.SurfaceSetMaterial(0, mat);

        Changed();
    }
}