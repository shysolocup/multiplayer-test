using Godot;
using System.Text;

[Tool]
[GlobalClass, Icon("")]
public partial class ShaderRect : ColorRect
{
    #nullable enable

    [Export]
    public Shader? Shader { 
        get {
            Material ??= new ShaderMaterial();
            
            if (ShaderMaterial is not null)
                return ShaderMaterial.Shader;

            return null;
        }
        set {
            if (ShaderMaterial is not null)
                ShaderMaterial.Shader = value;
        }
    }

    [Export]
    public ShaderMaterial? ShaderMaterial
    {
        get => (ShaderMaterial)Material;
        set => Material = value;
    }

    // Accepts either a StringName or a plain string and will try snake_case <-> PascalCase variants.
    public void ShaderSet(StringName parameter, Variant value)
        => ShaderSet(parameter.ToString(), value);

    public void ShaderSet(string parameter, Variant value)
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        // Try the provided name and a couple of normalized variants to account for C# and GDScript naming styles.
        SetInstanceShaderParameter(parameter, value);

        var snake = ToSnakeCase(parameter);
        if (snake != parameter)
            SetInstanceShaderParameter(snake, value);

        var pascal = ToPascalCase(parameter);
        if (pascal != parameter && pascal != snake)
            SetInstanceShaderParameter(pascal, value);
    }

    public Variant ShaderGet(StringName parameter) => ShaderGet(parameter.ToString());

    public Variant ShaderGet(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return new Variant();

        // Try original, then PascalCase, then snake_case.
        var v = GetInstanceShaderParameter(parameter);
        if (!IsVariantNullish(v))
            return v;

        var pascal = ToPascalCase(parameter);
        if (pascal != parameter)
        {
            v = GetInstanceShaderParameter(pascal);
            if (!IsVariantNullish(v))
                return v;
        }

        var snake = ToSnakeCase(parameter);
        if (snake != parameter)
        {
            v = GetInstanceShaderParameter(snake);
            if (!IsVariantNullish(v))
                return v;
        }

        return v;
    }

    private static bool IsVariantNullish(Variant v)
    {
        // Heuristic: treat empty or null-like Variant string representation as "not set".
        // This is intentionally conservative — if shading code legitimately sets an empty string this
        // may fall back to trying alternate names, which is generally harmless.
        try
        {
            var s = v.ToString();
            return string.IsNullOrEmpty(s);
        }
        catch
        {
            return false;
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && name[i - 1] != '_')
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }


    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var parts = name.Split('_');
        var sb = new StringBuilder();
        foreach (var p in parts)
        {
            if (string.IsNullOrEmpty(p))
                continue;
            sb.Append(char.ToUpperInvariant(p[0]));
            if (p.Length > 1)
                sb.Append(p[1..].ToLowerInvariant());
        }

        return sb.ToString();
    }


    public override void _Ready()
    {
        base._Ready();

        Material ??= new ShaderMaterial();

        MouseFilter = MouseFilterEnum.Ignore;

        // Vector2 basis = GetParentAreaSize(); // probably works

        // SetDeferred(Control.PropertyName.Size, basis);
        CallDeferred(Control.MethodName._SetLayoutMode, 1);
        CallDeferred(Control.MethodName.SetAnchorsPreset, (int)LayoutPreset.FullRect);
    }
}