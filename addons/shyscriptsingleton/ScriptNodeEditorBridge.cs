using Godot;

[Tool]
public partial class ScriptNodeEditorBridge : Node
{
	public static ShyScriptSingleton Plugin;

	public static void RequestCreateSource(ScriptNode node)
	{
		var baseControl = EditorInterface.Singleton.GetBaseControl();
		Plugin ??= baseControl.GetNodeOrNull<ShyScriptSingleton>("ShyScriptSingleton");
		Plugin?.OpenDialog(node);
	}
}
