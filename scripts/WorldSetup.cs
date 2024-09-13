using Godot;
using System;
public partial class WorldSetup : Node3D
{
    [Export] private PackedScene playerScene;

    public override void _Ready()
    {
        foreach (var item in Globals.PLAYERS)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();

            currentPlayer.Name = item.server_id.ToString();
            AddChild(currentPlayer);

            currentPlayer.GlobalPosition = new Vector3(GD.Randf() * 20, 10 , 10);
        }
    }

}

