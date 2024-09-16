using Godot;
using System;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

public partial class PlayerNetworkingCalls : Node
{
    // Only Gets Called For Player That is Damaged
    [Signal] public delegate void PlayerDamageUpdateEventHandler(PlayerInfo reciever, PlayerInfo sender);
    [Signal] public delegate void PlayerDropWeaponEventHandler(int loadout_index, PlayerInfo sender);
    [Signal] public delegate void PlayerShootEventHandler(PlayerInfo player_id);
    [Signal] public delegate void PlayerDeathEventHandler(PlayerInfo player_id);
    [Signal] public delegate void PlayerUpdateLoadoutEventHandler(PlayerInfo player_id, int current_loadout_index);

    internal void Damage(int damage, PlayerInfo reciever, PlayerInfo sender)
    {
        if (Multiplayer.IsServer())
        {
            networkedDamage(damage, reciever.server_id, sender.server_id);
        }
        else
        {
            RpcId(1, nameof(networkedDamage), damage, reciever.server_id, sender.server_id);
        }
    }

    internal void DropWeapon(Player player)
    {
        if (Multiplayer.IsServer())
        {
            networkedWeaponDrop(player.WEAPON_CONTROLLER.current_loadout_index, player.player_info.server_id);
        }
        else
        {
            RpcId(1, nameof(networkedWeaponDrop), player.WEAPON_CONTROLLER.current_loadout_index, player.player_info.server_id);
        }
    }

    internal void Shoot(Player player) 
    {
        if (Multiplayer.IsServer())
        {
            networkedPlayerShoot(player.player_info.server_id);
        }
        else
        {
            RpcId(1, nameof(networkedPlayerShoot), player.player_info.server_id);
        }
    }

    internal void UpdateLoadout(Player player)
    {
        if (Multiplayer.IsServer())
        {
            networkedUpdateLoadout(player.player_info.server_id, player.WEAPON_CONTROLLER.current_loadout_index,
                player.player_info.loadout[0], player.player_info.loadout[1], player.player_info.loadout[2]);
        }
        else
        {
            RpcId(1, nameof(networkedUpdateLoadout), player.player_info.server_id, player.WEAPON_CONTROLLER.current_loadout_index,
                player.player_info.loadout[0], player.player_info.loadout[1], player.player_info.loadout[2]);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void networkedDamage(int damage, int reciever_id, int sender_id)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networkedDamage), damage, reciever_id, sender_id);
        }

        var reciever_player = Globals.PLAYERS.Find(p => p.server_id == reciever_id); // we find the reciever of the damage
        var sender_player = Globals.PLAYERS.Find(p => p.server_id == sender_id); // we find the sender of the damage

        if (reciever_player != null && sender_player != null)
        {
            if (reciever_player.player_team != sender_player.player_team || reciever_player.player_team == Team.None)
            {
                reciever_player.health -= damage;
                if (reciever_player.health <= 0)
                {
                    reciever_player.deaths++;
                    sender_player.kills++;
                    sender_player.money += 300;
                }
            }
            else
            {
                Globals.PlayerUI.debug().debug_err(sender_player.Name + " shot his own teamate " + reciever_player.Name);
            }
        }
        else
        {
            Globals.PlayerUI.debug().debug_err("Sender ID: " + sender_id + " To Reciever ID: " + reciever_id);
            Globals.PlayerUI.debug().debug_err("Null Player Found");
        }

        EmitSignal(nameof(PlayerDamageUpdate), reciever_player, sender_player);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void networkedWeaponDrop(int loadout_index, int player_id)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networkedWeaponDrop), loadout_index, player_id);
        }
        var player = Globals.PLAYERS.Find(p => p.server_id == player_id);
        //Globals.PlayerUI.debug().debug_message(player.Name + " Dropped a " + player.loadout[loadout_index]);
        EmitSignal(nameof(PlayerDropWeapon), loadout_index, player);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void networkedPlayerShoot(int player_id)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networkedPlayerShoot), player_id);
        }

        var player = Globals.PLAYERS.Find(p => p.server_id == player_id);

        //Globals.PlayerUI.debug().debug_message(player.Name + " Shooting");

        EmitSignal(nameof(PlayerShoot), player);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void networkedUpdateLoadout(int player_id, int current_loadout_index, StringName loadout1, StringName loadout2, StringName loadout3)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networkedUpdateLoadout), player_id, current_loadout_index,
                loadout1, loadout2, loadout3);
        }

        var player = Globals.PLAYERS.Find(p => p.server_id == player_id);
        player.loadout[0] = loadout1;
        player.loadout[1] = loadout2;
        player.loadout[2] = loadout3;

        EmitSignal(nameof(PlayerUpdateLoadout), player, current_loadout_index);
    }
}
