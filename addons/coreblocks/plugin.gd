### COREBLOCKS BY SHYSOLOCUP ###

@tool
extends EditorPlugin

const base: String = "res://addons/coreblocks/"
var dock

#region enter tree
func _enter_tree():
	# if c# is enabled then instantiate the plugin scene
	# if not direct them to the download website

	# there's like no documentation on this but one of these has to work
	if ProjectSettings.get_setting("dotnet/project/assembly_name", "") == "" and not OS.has_feature("dotnet"):
		push_error(".NET C# is not enabled, download at https://dotnet.microsoft.com/en-us/download/dotnet/8.0")

	var editor_interface = get_editor_interface()
	var main_screen = editor_interface.get_editor_main_screen()

	for child in main_screen.get_children():
		if child.name == "Coreblocks":
			child.queue_free()
			break

	dock = preload(base + "scenes/plugin.tscn").instantiate()

	dock.set_deferred("Plugin", self);

	# also add hidden and extension stuff to autoload

	add_autoload_singleton("hidden", base + "scenes/hidden_singletons.tscn")
	add_autoload_singleton("extensions", base + "core/libs/Ext.cs")

	add_control_to_dock(DOCK_SLOT_LEFT_BL, dock)

	var maker: Button = dock.get_node("./container/maker");
	var dialog: ConfirmationDialog = dock.get_node("./confirmMaker");	
	maker.pressed.connect(dialog.show);
	dialog.confirmed.connect(make_new_scene);

#endregion


func query(parent: Node, systemName: String) -> Node:
	var query = parent.find_children("*", systemName, true);
	return query[0] if not query.is_empty() else null;


func make_new_scene():
	var interface = get_editor_interface();
	var game = interface.get_edited_scene_root();
	
	#region workspace

	# make Workspace
	var workspace = query(game, "Workspace");
	if (workspace is not Workspace):
		var _script: CSharpScript = preload(base + "core/Workspace.cs")

		workspace = Node3D.new();
		workspace.name = "workspace";

		game.add_child(workspace as Variant);
		workspace.owner = game;

		var spawn = Marker3D.new();
		spawn.name = "spawn";
		spawn.gizmo_extents = 0.25;

		workspace.add_child(spawn);
		spawn.owner = game;

		workspace.set_script(_script);

	# make Characters
	var characters = query(game, "Characters");
	if (characters is not Characters):
		
		var _script: CSharpScript = preload(base + "core/Characters.cs")
		characters = Node3D.new();
		characters.name = "characters";

		workspace.add_child(characters);
		characters.owner = game;

		characters.set_script(_script);

	# spawn MapSystem
	var map = query(game, "MapSystem");
	if (map is not MapSystem):

		var _script: CSharpScript = preload(base + "core/MapSystem.cs")
		map = Node3D.new();
		map.name = "map";

		workspace.add_child(map);
		map.owner = game;

		map.set_script(_script);
	
	# spawn LightingSystem
	var lighting = query(game, "LightingSystem");
	if (lighting is not LightingSystem):

		var _script: CSharpScript = preload(base + "core/LightingSystem.cs")
		lighting = Node3D.new();
		lighting.name = "lighting";

		workspace.add_child(lighting);
		lighting.owner = game;

		lighting.set_script(_script);

	# spawn AudioSystem
	var audios = query(game, "AudioSystem");
	if (audios is not AudioSystem):
		var _script: CSharpScript = preload(base + "core/AudioSystem.cs")
		audios = Node3D.new();
		audios.name = "audios"

		workspace.add_child(audios);
		audios.owner = game;

		audios.set_script(_script);

	#endregion
	#region client

	# spawn Client
	var client = query(game, "Client");
	if (client is not Client):

		var _script: CSharpScript = preload(base + "core/Client.cs")
		client = Node.new();
		client.name = "client";

		game.add_child(client as Variant)
		client.owner = game;

		client.set_script(_script);
	
	# spawn CameraSystem
	var cameras = query(game, "CameraSystem");
	if (cameras is not CameraSystem):

		var _script: CSharpScript = preload(base + "core/CameraSystem.cs")
		cameras = Node3D.new();
		cameras.name = "cameras";

		client.add_child(cameras);
		cameras.owner = game;

		var freecam = Camera3D.new();
		freecam.name = "freecam";
		cameras.add_child(freecam);
		freecam.owner = game;

		var firstPerson = Camera3D.new();
		firstPerson.name = "firstPerson";
		cameras.add_child(firstPerson);
		firstPerson.owner = game;

		var thirdPerson = Camera3D.new();
		thirdPerson.name = "thirdPerson";
		cameras.add_child(thirdPerson);
		thirdPerson.owner = game;

		cameras.set_script(_script);

		cameras.set("CurrentCamera", thirdPerson);

	# spawn GuiSystem
	var gui = query(game, "GuiSystem");
	if (gui is not GuiSystem):

		var _script: CSharpScript = preload(base + "core/GuiSystem.cs")
		gui = Control.new();
		gui.name = "gui";

		client.add_child(gui);
		gui.owner = game;

		gui.set_script(_script);
	
	# spawn CoreGui
	var coreGui = query(game, "CoreGui");
	if (coreGui is not CoreGui):

		coreGui = preload("res://src/assets/scenes/core/core_gui.tscn").instantiate();
		coreGui.name = "coreGui";

		gui.add_child(coreGui);
		coreGui.owner = game;

	# spawn ShaderSystem
	var shaders = query(game, "ShaderSystem");
	if (shaders is not ShaderSystem):

		shaders = preload("res://src/assets/scenes/shaders.tscn").instantiate();
		shaders.name = "shaders";

		gui.add_child(shaders);
		shaders.owner = game;

	# spawn ClientScriptSystem
	var clientScripts = query(game, "ClientScriptSystem");
	if (clientScripts is not ClientScriptSystem):

		clientScripts = preload("res://src/assets/scenes/scripts/client_scripts.tscn").instantiate();
		clientScripts.name = "scripts";

		client.add_child(clientScripts);
		clientScripts.owner = game;

	# spawn DiscordSystem
	var discord = query(game, "DiscordRPCNode");
	if (discord is not DiscordRPCNode):
		
		discord = DiscordRPCNode.new();
		discord.name = "discord";

		client.add_child(discord);
		discord.owner = game;

	#endregion
	#region server

	# spawn Server
	var server = query(game, "Server");
	if (server is not Server):

		var _script: CSharpScript = preload(base + "core/Server.cs")
		server = Node.new();
		server.name = "server";

		game.add_child(server);
		server.owner = game;

		server.set_script(_script);

	# spawn ServerScriptSystem
	var serverScripts = query(game, "ServerScriptSystem");
	if (serverScripts is not ServerScriptSystem):
		
		serverScripts = preload("res://src/assets/scenes/scripts/server_scripts.tscn").instantiate();
		serverScripts.name = "scripts";

		server.add_child(serverScripts);
		serverScripts.owner = game;

	#endregion

	# spawn GlobalStorage
	var globalStorage = query(game, "GlobalStorage");
	if (globalStorage is not GlobalStorage):
		
		var _script: CSharpScript = preload(base + "core/GlobalStorage.cs")
		globalStorage = Node.new();
		globalStorage.name = "global";

		game.add_child(globalStorage);
		globalStorage.owner = game;

		globalStorage.set_script(_script);

	# spawn Players
	var players = query(game, "Players");
	if (players is not Replicator):
		
		var _script: CSharpScript = preload(base + "core/Players.cs")
		players = Node.new();
		players.name = "players";
		players.set_script(_script);

		game.add_child(players);
		players.owner = game;

	# spawn Replicator
	var replicator = query(game, "Replicator");
	if (replicator is not Replicator):
		
		var _script: CSharpScript = preload(base + "core/Replicator.cs")
		replicator = Node.new();
		replicator.name = "replicator";

		game.add_child(replicator);
		replicator.owner = game;

		var playerSpawner = MultiplayerSpawner.new();
		playerSpawner.name = "playerSpawner"
		playerSpawner.spawn_path = "../../players";
		replicator.add_child(playerSpawner);
		playerSpawner.owner = game;

		var charaSpawner = MultiplayerSpawner.new();
		charaSpawner.name = "characterSpawner"
		charaSpawner.spawn_path = "../../workspace/characters"
		replicator.add_child(charaSpawner);
		charaSpawner.owner = game;

		replicator.set_script(_script);

	# make Game last
	if (game is not Game):

		var _script: CSharpScript = preload(base + "core/Game.cs")

		game.name = "game";
		game.set_script(_script);


func _exit_tree():
	remove_autoload_singleton("hidden")
	remove_autoload_singleton("extensions")

	var editor_interface = get_editor_interface()
	var main_screen = editor_interface.get_editor_main_screen()

	for child in main_screen.get_children():
		if child.name == "Coreblocks":
			child.queue_free()
			break

	if dock != null:
		remove_control_from_docks(dock)
		dock.free()