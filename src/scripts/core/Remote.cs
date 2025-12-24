using Godot;
using Godot.Collections;

[GlobalClass, Icon("uid://b1mrpempxy0vk")]
public partial class Remote : Node
{
	public delegate void OnClientEvent(params object[] args);
	private event OnClientEvent ClientEvent;

	public delegate void OnServerEvent(Player player, params object[] args);
	private event OnServerEvent ServerEvent;


	public override void _Ready()
	{
		base._Ready();
	}


	#region private base methods


	private void _FireServer(Player player, params Variant[] args)
	{
		if (Multiplayer.IsServer())
		{
			ServerEvent?.Invoke(Client.LocalPlayer, args);
		}
	}

	public void _FireClient(Variant[] args)
	{
		ClientEvent?.Invoke(args);
	}

	private void _FireAllClients(params Variant[] args)
	{
		if (!Multiplayer.IsServer())
		{
			ClientEvent?.Invoke(args);
		}
	}


    #endregion


    #region reliable

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Reliable ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireClient(Player player, params Variant[] args) => RpcId(player.GetId(), nameof(_FireClient), args);

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void FireClient(long id, params Variant[] args) => RpcId(id, nameof(_FireClient), args);


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void FireAllClients(params Variant[] args) => _FireAllClients(args);


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void FireServer(params Variant[] args) => _FireServer(Client.LocalPlayer, args);


	#endregion
	#region unreliable
	
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// Unreliable ///
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void FireAllClientsUnreliably(params Variant[] args) => _FireAllClients(args);


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void FireClientUnreliably(Player player, params Variant[] args) => RpcId(player.GetId(), nameof(_FireClient), args);

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void FireClientUnreliably(long id, params Variant[] args) => RpcId(id, nameof(_FireClient), args);


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void FireServerUnreliably(params Variant[] args) => _FireServer(Client.LocalPlayer, args);


	#endregion
}
