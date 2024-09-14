using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node3D
{
    private List<Player> m_players = new List<Player>();

    private int current_spectator = 0;
    private bool current_spectating = false;

    [Export] private PackedScene playerScene;
    [Export] private SpawnPoints spawnPoints;

    public override void _Ready()
    {
        Globals.gameManager = this;
        SpawnPlayers();
    }


    public override void _Input(InputEvent @event)
    {
        if (current_spectating)
        {
            if (Input.IsActionJustPressed("attack")) // mouse button 1 pressed
            {
                // Ensure there are players to spectate
                if (m_players.Count == 0)
                    return;

                // Start looking from the next spectator index
                int startIndex = current_spectator;
                bool foundValidTeammate = false;

                // Loop through all players once to find the next valid teammate
                for (int i = 0; i < m_players.Count; i++)
                {
                    // Move to the next spectator, looping back if at the end
                    current_spectator = (current_spectator + 1) % m_players.Count;

                    var currentPlayer = m_players[current_spectator].player_info;

                    // Check if the current player is a valid teammate and alive
                    if (currentPlayer.player_team == Globals.localPlayerInfo.player_team && currentPlayer.health > 0)
                    {
                        foundValidTeammate = true;
                        break; // Exit the loop once a valid teammate is found
                    }
                }

                // Set camera to the found valid teammate, or fallback to the current if none found
                if (foundValidTeammate)
                {
                    SetCamera(m_players[current_spectator]);
                }
                else
                {
                    // Optional: Handle the case where no valid teammate is found
                    GD.Print("No valid teammate found to spectate.");
                }
            }
        }
    }

    public void HandlePlayerDead(Player player)
    {
        switch (Globals.game_mode)
        {
            case 0: // 5v5
                DisablePlayer(player);
                SpectateTeamate();
                break;
            case 1: // Free For All
                player.ResetAttributes();
                player.Position = spawnPoints.GetFFASpawnPoint().Position;
                break;
            case 2: // Zombies
                break;
        }
    }

    public void SpectateTeamate()
    {
        foreach (Player current_player in m_players)
        {
            if (current_player.player_info.player_team == Globals.localPlayerInfo.player_team && current_player.player_info.health > 0)
            {
                SetCamera(current_player);
                current_spectator = m_players.IndexOf(current_player);
                break;
            }
        }
    }

    public void SetCamera(Player player)
    {
        player._camera.MakeCurrent();
    }

    public void DisablePlayer(Player player)
    {
        current_spectating = true;

        // Set Node Invisible
        player.Hide();

        // Set Player Under Map
        player.Position = new Vector3(-100, -100, -100);

        // Disable its Processing
        player.SetProcess(false);
        player.SetPhysicsProcess(false);
        player.SetProcessInput(false);
        foreach (Node child in player.GetChildren())
        {
            child.SetProcess(false);
        }
    }

    public void EnablePlayer(Player player)
    {
        current_spectating = false;

        // Set Node Invisible
        player.Show();

        // Disable its Processing
        player.SetProcess(true);
        player.SetPhysicsProcess(true);
        player.SetProcessInput(true);
        foreach (Node child in player.GetChildren())
        {
            child.SetProcess(true);
        }

        player._camera.MakeCurrent();
    }

    public void SpawnPlayer(PlayerInfo player)
    {
        Player currentPlayer = playerScene.Instantiate<Player>();
        m_players.Add(currentPlayer); // we can handle player through this list

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



