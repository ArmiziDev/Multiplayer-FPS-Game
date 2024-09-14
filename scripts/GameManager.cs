using Godot;
using System;
public partial class GameManager : Node3D
{
    [Export] private PackedScene playerScene;

    public int game_mode;

    public override void _Ready()
    {
        SpawnPlayers();
    }

    public void SetGameMode(int game_mode)
    {
        this.game_mode = game_mode;
    }

    public void SpawnPlayers()
    {
        foreach (var player in Globals.PLAYERS)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();

            currentPlayer.Name = player.server_id.ToString();
            currentPlayer.player_info = player; // this is where we initialize the player info

            if (game_mode == 1)
            {
                currentPlayer.player_info.player_team = Team.None;
            }

            AddChild(currentPlayer);

            currentPlayer.GlobalPosition = new Vector3(GD.Randf() * 20, 10, 10);
        }
    }
}



