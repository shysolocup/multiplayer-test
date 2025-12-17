using Godot;



[Tool]
public partial class ShyScriptSingleton : EditorPlugin
{
	private EditorFileDialog _dialog;
	private ScriptNode _node;

    public override void _Ready()
    {
        base._Ready();
		ScriptNodeEditorBridge.Plugin = this;
    }

	public override void _EnterTree()
	{
		ScriptNodeEditorBridge.Plugin = this;

		_dialog = new EditorFileDialog
		{
			FileMode = EditorFileDialog.FileModeEnum.SaveFile,
			Access = EditorFileDialog.AccessEnum.Filesystem
		};

		_dialog.FileSelected += path =>
		{
			_node.Name = System.IO.Path.GetFileNameWithoutExtension(path);
			_node?.CallDeferred("CreateFile", path);
		};

		EditorInterface.Singleton.GetBaseControl().AddChild(_dialog);
	}


	public void OpenDialog(ScriptNode node)
	{
		GD.Print("a");
        _dialog.Filters = [
            node.FileExtension switch
            {
                0 => "*.cs ; C# Files",
                1 => "*.gd ; GDScript Files",
                2 => "*.cpp ; C++ Files",
                3 => "*.lua ; Lua Files",
                4 => "*.py ; Python Files",
                5 => "*.rs ; Rust Files",
                _ => "*.txt ; Text Files"
            }
        ];
		_node = node;
		_dialog.CallDeferred(Window.MethodName.PopupCentered);
	}
}
