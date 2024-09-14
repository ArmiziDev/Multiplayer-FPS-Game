using Godot;
using System;
public partial class GameManager : Node3D
{
    [Export] private PackedScene playerScene;
    [Export] private SpawnPoints spawnPoints;

    public override void _Ready()
    {
        Globals.gameManager = this;
        SpawnPlayers();
    }

    public void HandlePlayerDead(Player player)
    {
        switch (Globals.game_mode)
        {
            case 0: // 5v5
                break;
            case 1: // Free For All
                player.ResetAttributes();
                player.Position = spawnPoints.GetFFASpawnPoint().Position;
                break;
            case 2: // Zombies
                break;
        }
    }

    public void SpawnPlayer(PlayerInfo player)
    {
        Player currentPlayer = playerScene.Instantiate<Player>();
        currentPlayer.Name = player.server_id.ToString();
        currentPlayer.player_info = player; // this is where we initialize the player info
        player.health = 100;

        switch (Globals.game_mode)
        {
            case 0: // 5v5
                break;
            case 1: // Free For All
                player.player_team = Team.None;
                currentPlayer.Position = spawnPoints.GetFFASpawnPoint().Position;
                break;
            case 2: // Zombies
                break;
        }

        AddChild(currentPlayer);
    }

    public void SpawnPlayers()
    {
        // First check for any extra players
        foreach (var node in GetTree().Root.GetChildren())
        {
            if (node is Player)
            {
                // Remove Player
                node.QueueFree();
            }
        }

        foreach (var player in Globals.PLAYERS)
        {
            SpawnPlayer(player);
        }
    }


}



