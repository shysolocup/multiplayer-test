public static class ScriptNodeEditorBridge
{
	public static ShyScriptSingleton Plugin;

	public static void RequestCreateSource(ScriptNode node)
	{
		Plugin?.OpenDialog(node);
	}
}
