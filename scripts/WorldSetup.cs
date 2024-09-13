using Godot;
using System;
public partial class WorldSetup : Node3D
{
    [Export] private PackedScene playerScene;

    public override void _Ready()
    {
        foreach (var player in Globals.PLAYERS)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();

            currentPlayer.Name = player.server_id.ToString();
            currentPlayer.player_info = player; // this is where we initialize the player info

            AddChild(currentPlayer);

            currentPlayer.GlobalPosition = new Vector3(GD.Randf() * 20, 10 , 10);
        }
    }

}

