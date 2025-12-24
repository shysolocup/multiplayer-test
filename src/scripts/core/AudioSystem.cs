using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass, Icon("uid://cpsdgcaao4new")]
public partial class AudioSystem : Singleton3D<AudioSystem>
{

	/// <summary>
	/// Play a relative audio by directory eg: AudioSystem.Play("Walk")
	/// </summary>
	/// <param name="directory"></param>
	/// <returns></returns>
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public static async Task<AudioStreamPlayer3D> Play(string directory)
	{
		var audio = await Get($"./{directory}");
		audio.Play();
		return audio;
	}

	/// <summary>
	/// Gets relative audios by directory eg: AudioSystem.Get("Walk")
	/// </summary>
	/// <param name="directory"></param>
	/// <returns></returns>
	public static async Task<AudioStreamPlayer3D> Get(string directory)
	{
		var audios = await Instance();
		var audio = audios.GetNode<AudioStreamPlayer3D>(directory);
		return audio;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
