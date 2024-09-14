using Godot;
using System;

public partial class PlayerNetworkingCalls : Node
{
    [Signal] public delegate void PlayerInformationUpdateEventHandler();

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
                }
            }
            else
            {
                Globals.debug.debug_err(sender_player.Name + " shot his own teamate " + reciever_player.Name);
            }
        }
        else
        {
            Globals.debug.debug_err("Sender ID: " + sender_id + " To Reciever ID: " + reciever_id);
            Globals.debug.debug_err("Null Player Found");
        }

        EmitSignal(nameof(PlayerInformationUpdate));
    }
}
