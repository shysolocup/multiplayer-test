using System.Text.RegularExpressions;
using Godot;

[GlobalClass]
public partial class JsonLib : Singleton<JsonLib> {

    /// <summary>
    /// utility function for <c>.jsonc</c> files
    /// </summary>
    public string Jsonc(string jsonc)
        => LineCommentGuh().Replace(BlockCommentGuh().Replace(jsonc, ""), "");


    public string Stringify(Variant data) 
        => Json.Stringify(data);


    [GeneratedRegex(@"//.*?$", RegexOptions.Multiline)]
    private partial Regex LineCommentGuh();


    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private partial Regex BlockCommentGuh();
}