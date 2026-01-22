### COREBLOCKS BY SHYSOLOCUP ###

@tool
extends EditorPlugin

const base: String = "res://addons/coreblocks/"
var dock


func _enter_tree():
	# if c# is enabled then instantiate the plugin scene
	# if not direct them to the download website

	# there's like no documentation on this but one of these has to work
	if ProjectSettings.get_setting("dotnet/project/assembly_name", "") == "" and not OS.has_feature("dotnet"):
		push_error(".NET C# is not enabled, download at https://dotnet.microsoft.com/en-us/download/dotnet/8.0")

	dock = preload(base + "scenes/plugin.tscn").instantiate()

	dock.set_deferred("Plugin", self);

	# also add hidden and extension stuff to autoload

	add_autoload_singleton("hidden", base + "scenes/hidden_singletons.tscn")
	add_autoload_singleton("extensions", base + "core/libs/Ext.cs")

	add_control_to_dock(DOCK_SLOT_LEFT_BL, dock)


func _exit_tree():
	remove_autoload_singleton("hidden")
	remove_autoload_singleton("extensions")

	if dock != null:
		remove_control_from_docks(dock)
		dock.free()