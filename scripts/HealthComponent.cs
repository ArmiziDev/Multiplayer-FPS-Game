using Godot;
using System;
using System.Reflection;

public partial class HealthComponent : Node
{
	public Player player;
	[Signal] public delegate void DamageEventHandler();
	public Node parent;
	public void InitializeHealthComponent(Player player)
	{
		this.player = player;
		parent = GetParent();
		parent.Connect("ready", new Callable(this, nameof(connect_parent)));
	}

	private void connect_parent()
	{
		parent.AddUserSignal(HealthComponent.SignalName.Damage);
		parent.Connect(HealthComponent.SignalName.Damage, new Callable(this, nameof(damage)));
	}

	private void damage(int amount) // if we pass in an int
	{
		player.player_info.health -= amount;
		Globals.debug.debug_message(parent.Name + ": Health " + player.player_info.health);
		check_death();
	}

	private void damage(int amount, PlayerInfo playerInfo)
	{
		//Globals.debug.debug_message("Shooter: " + enemy_team + " Reciever: " + team);
		if (playerInfo.player_team != player.player_info.player_team || player.player_info.player_team == Team.None) /// if player is on opposite team or on none
		{
			damage(amount);
		}
		else
		{
			Globals.debug.debug_err("Can't Shoot Teamate!!!");
		}
	}

	private void check_death()
	{
		if (player.player_info.health <= 0)
		{
			// Check if the parent has a "Die" method
			MethodInfo dieMethod = parent.GetType().GetMethod("Die");

			if (dieMethod != null)
			{
				// If it exists, invoke the parent's Die method
				dieMethod.Invoke(parent, null);
			}
			else
			{
				// If no Die method exists, run the default logic
				parent.QueueFree();
			}
		}
	}

	
}
