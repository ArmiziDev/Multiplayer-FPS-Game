using Godot;
using System;

public partial class MultiplayerController : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

    private void PeerDisconnected(long id)
    {
        throw new NotImplementedException();
    }

    private void PeerConnected(long id)
    {
        throw new NotImplementedException();
    }

    private void ConnectionFailed()
    {
        throw new NotImplementedException();
    }

    private void ConnectedToServer()
    {
        throw new NotImplementedException();
    }
	
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
