using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FileLib : Singleton<FileLib> {

    public void Open(StringName path)
        => FileAccess.Open(path, FileAccess.ModeFlags.Read);

    public string Read(StringName path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        return file.GetAsText();
    }

    public void Write(StringName path, Dictionary data)
    {
        string json = Json.Stringify(data, "\t");

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    public void Write(StringName path, string data)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(data);
    }
}