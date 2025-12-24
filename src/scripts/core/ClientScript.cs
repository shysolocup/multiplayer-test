using Godot;
using System;

[Tool]
[GlobalClass, Icon("uid://dajl85q1ep7j4")]
public partial class ClientScript : ScriptBase
{

	#region dependencies

	public Client client;
	public AudioSystem audios;
	public CameraSystem cameras;
	public Characters characters;
	public Game game;
	public GlobalStorage global;
	public LightingSystem lighting;
	public MapSystem maps;
	public Players players;
	public ClientScriptSystem clientScripts;
	public Workspace workspace;
	public GuiSystem gui;
	public ShaderSystem shaders;

	public override async void _Ready()
	{
		client = await Client.Instance();
		audios = await AudioSystem.Instance();
		cameras = await CameraSystem.Instance();
		characters = await Characters.Instance();
		game = await Game.Instance();
		global = await GlobalStorage.Instance();
		lighting = await LightingSystem.Instance();
		maps = await MapSystem.Instance();
		players = await Players.Instance();
		clientScripts = await ClientScriptSystem.Instance();
		workspace = await Workspace.Instance();
		gui = await GuiSystem.Instance();
		shaders = await ShaderSystem.Instance();

		// it's specifically just in this order so ready fires before processes
		// this might lead to issues but we ball
		OnReady();
		ScriptReady = true;
	}

	#endregion
}