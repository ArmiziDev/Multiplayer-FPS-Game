using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node3D
{
    double calculate_ping = 0;
    bool check_ping = false;

    int red_team_score = 0;
    int blue_team_score = 0;

    Timer round_time_left;

    private List<Player> m_players = new List<Player>();

    private int current_spectator = 0;
    private bool current_spectating = false;

    [Export] private PackedScene playerScene;
    [Export] private SpawnPoints spawnPoints;

    public override void _Ready()
    {
        Globals.gameManager = this;

        round_time_left = GetNode<Timer>("round_time_left");

        PreloadWeapons();
        SpawnPlayers();

        // Updating Game Manager UI Elements
        Globals.PlayerUI.playerUI().UpdateUI("RedTeamScore", red_team_score);
        Globals.PlayerUI.playerUI().UpdateUI("BlueTeamScore", blue_team_score);
        Globals.PlayerUI.playerUI().UpdateUI("RoundTimeDisplay", round_time_left.TimeLeft);
    }

    private void PreloadWeapons()
    {
        AddWeaponToDictionary("res://meshes/weapons/weapon_pack/hand/EMPTY_HAND.tres");
        AddWeaponToDictionary("res://meshes/weapons/weapon_pack/Rifles/AR15_19/weapon_ar15.tres");
        AddWeaponToDictionary("res://meshes/weapons/weapon_pack/Pistols/DEAGLE_125/WeaponDeagle.tres");
        AddWeaponToDictionary("res://meshes/weapons/weapon_pack/Rifles/AK-556_28/weapon_ak556.tres");
        AddWeaponToDictionary("res://meshes/weapons/weapon_pack/SMG/HK_MP7/Weapon_MP7.tres");
    }

    private void AddWeaponToDictionary(String location)
    {
        Weapons current_weapon = (Weapons)GD.Load(location);
        Globals.weaponDictionary[current_weapon.name] = current_weapon;
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

    public override void _Process(double delta)
    {
        if (check_ping)
        {
            calculate_ping += delta;
            Globals.PlayerUI.debug().update_debug_property("Ping", calculate_ping);
        }
        else
        {
            Globals.PlayerUI?.debug().update_debug_property("Ping", calculate_ping);
            calculate_ping = 0;
        }
    }

    private void _on_round_end_timeout()
    {
        Globals.PlayerUI.debug().debug_message("Round Ended");
    }

    public void Round5v5End(Team winning_team)
    {
        switch (winning_team)
        {
            case Team.Red:
                red_team_score++;
                Globals.PlayerUI.playerUI().UpdateUI("RedTeamScore", red_team_score);
                break;
            case Team.Blue:
                blue_team_score++;
                Globals.PlayerUI.playerUI().UpdateUI("RedTeamScore", blue_team_score);
                break;
        }

        foreach (Player player in m_players)
        {
            player.ResetAttributes();
            EnablePlayer(player);

            switch (player.player_info.player_team)
            {
                case Team.Red:
                    player.Position = spawnPoints.GetRedTeamSpawnPoint().Position;
                    break;
                case Team.Blue:
                    player.Position = spawnPoints.GetBlueTeamSpawnPoint().Position;
                    break;
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
                CheckForRoundEnd();
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

        // Add Signals
        currentPlayer.playerNetworkingCalls.PlayerDropWeapon += NetworkPlayerDropWeapon;
        currentPlayer.playerNetworkingCalls.PlayerShoot += NetworkPlayerShoot;
        currentPlayer.playerNetworkingCalls.PlayerUpdateLoadout += NetworkUpdateLoadout;
        currentPlayer.playerNetworkingCalls.PlayerWeaponBought += NetworkWeaponBought;
        currentPlayer.playerNetworkingCalls.PlayerDropWeaponKeepOriginal += NetworkDropWeaponKeepOriginal;
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
    public void CheckForRoundEnd()
    {
        if (AreAllPlayersDead(Team.Red))
        {
            Round5v5End(Team.Blue); // Blue team wins
        }
        else if (AreAllPlayersDead(Team.Blue))
        {
            Round5v5End(Team.Red); // Red team wins
        }
    }
    public bool AreAllPlayersDead(Team team)
    {
        // Loop through all players and check if any from the specified team are still alive
        foreach (Player player in m_players)
        {
            if (player.player_info.player_team == team && player.player_info.health > 0)
            {
                return false; // There's at least one player alive in this team
            }
        }
        return true; // All players from the specified team are dead
    }

    public void NetworkPlayerDropWeapon(int loadout_index, PlayerInfo player)
    {
        Player reciever_player = m_players.Find(p => p.player_info.server_id == player.server_id);
        reciever_player.WEAPON_CONTROLLER.DropCurrentWeapon();
    }

    public void NetworkPlayerShoot(PlayerInfo shooter_player)
    {
        Player reciever_player = m_players.Find(p => p.player_info.server_id == shooter_player.server_id);
        reciever_player.WEAPON_CONTROLLER._visual_weapon_fire();
    }

    public void NetworkUpdateLoadout(PlayerInfo player, int current_loadout_index)
    {
        Player current_player = m_players.Find(p => p.player_info.server_id == player.server_id);
        current_player.WEAPON_CONTROLLER.current_loadout_index = current_loadout_index;
        current_player.WEAPON_CONTROLLER.LoadWeapon();

        if (player.server_id == Globals.localPlayerInfo.server_id)
        {
            Globals.PlayerUI.playerUI().UpdateUI("Loadout1", "1: " + player.loadout[0]);
            Globals.PlayerUI.playerUI().UpdateUI("Loadout2", "2: " + player.loadout[1]);
            Globals.PlayerUI.playerUI().UpdateUI("Loadout3", "3: " + player.loadout[2]);
        }
    }

    public void NetworkWeaponBought(PlayerInfo player, StringName weapon_name)
    {
        Player current_player = m_players.Find(p => p.player_info.server_id == player.server_id);

        // Get Weapon from Global Weapon Dictionary
        Weapons weapon = Globals.weaponDictionary[weapon_name];

        if (weapon.gun_class == Weapons.GunClass.Rifle || weapon.gun_class == Weapons.GunClass.Sniper || weapon.gun_class == Weapons.GunClass.Shotgun || weapon.gun_class == Weapons.GunClass.SMG)
        {
            current_player.WEAPON_CONTROLLER.current_loadout_index = 0;
            current_player.WEAPON_CONTROLLER.DropWeaponKeepOriginal(0);
            player.loadout[0] = weapon_name;
        }
        else if (weapon.gun_class == Weapons.GunClass.Pistol)
        {
            current_player.WEAPON_CONTROLLER.current_loadout_index = 1;
            current_player.WEAPON_CONTROLLER.DropWeaponKeepOriginal(1);
            player.loadout[1] = weapon_name;
        }

        current_player.playerNetworkingCalls.UpdateLoadout(current_player);
    }

    public void NetworkDropWeaponKeepOriginal(PlayerInfo player, int loadout_index)
    {
        Player current_player = m_players.Find(p => p.player_info.server_id == player.server_id);

        current_player.WEAPON_CONTROLLER.DropWeaponKeepOriginal(loadout_index);
    }

    private void _on_check_ping_timeout()
    {
        check_ping = true;
        if (Multiplayer.GetUniqueId() != 1)
            RpcId(1, nameof(_PingServer), Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void _PingServer(int sender_id)
    {
        Globals.PlayerUI.debug().debug_message("Server Pinged From" +  sender_id);
        RpcId(sender_id, nameof(_PingClient));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void _PingClient()
    {
        Globals.PlayerUI.debug().debug_message("Server Pinged " + Globals.localPlayerInfo.Name);
        check_ping = false;
    }
}