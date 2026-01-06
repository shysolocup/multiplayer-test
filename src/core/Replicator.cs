using System.Threading.Tasks;
using Core;
using Godot;

[GlobalClass, Icon("uid://cg4mlqb2vd8jm")]
public partial class Replicator : Singleton<Replicator>
{

    private Server server { get; set; }
    private Client client { get; set; }

    public override async void _Ready()
    {
        base._Ready();

        server = await Server.Instance();
        client = await Client.Instance();
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public async Task Host()
	{
        var peer = Server.GetPeer();

		NodeTunnelBridge.Host(peer);

		// await to be connected
		await NodeTunnelBridge.Hosting(peer);

        var id = NodeTunnelBridge.GetOnlineId(peer);

		Server.HostId = id;
        // DisplayServer.ClipboardSet(HostId.ToString());

        server.EmitSignal(Server.SignalName.StartedHosting, id);

		GD.PushWarning($"started hosting at id {id}");	
	}


    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public async Task Join(string hostId)
	{
        var peer = Server.GetPeer();

		GD.PushWarning($"trying to join {hostId}");	
		NodeTunnelBridge.Join(peer, hostId);

		await NodeTunnelBridge.Joined(peer);

		var id = Server.GetId();

		GD.PushWarning($"peer {id} has joined with the function");	
	}
}
