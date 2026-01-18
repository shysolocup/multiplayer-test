using Godot;
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
	public AudioStreamPlayer3D Play(string directory)
	{
		var audio = Get($"./{directory}");
		audio.Play();
		return audio;
	}

	/// <summary>
	/// Gets relative audios by directory eg: AudioSystem.Get("Walk")
	/// </summary>
	/// <param name="directory"></param>
	/// <returns></returns>
	public AudioStreamPlayer3D Get(string directory)
	{
		var audio = GetNode<AudioStreamPlayer3D>(directory);
		return audio;
	}
}
