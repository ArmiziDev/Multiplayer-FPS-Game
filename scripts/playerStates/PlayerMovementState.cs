using Godot;
using System;

public partial class PlayerMovementState : State
{
    public Player player;
    public WeaponControllerSingleMesh weapon;
    public float acceleration; // Player State Set Accel
    public float deceleration; // Player State Set Decel
    public float speed; // Player State Set Speed
    public float input_multiplier;

    public void InitializeState(Player player)
    {
        this.player = player;
        weapon = player.WEAPON_CONTROLLER;
        acceleration = player.ACCELERATION;
        deceleration = player.DECELERATION;
        speed = player._speed;
        input_multiplier = 1.0f;
    }

    public override void Update(float delta)
    {
        if (player == null) return;

        UpdateGeneralInputs(delta);

        player.UpdateGravity(delta);
        player.UpdateInput(speed, acceleration, deceleration, input_multiplier);
        player.UpdateVelocity();
        player.run_raycast();

        update_debug();
    }

    public void UpdateGeneralInputs(float delta)
    {
        if (player.multiplayerSynchronizer.GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) return;
        if (Input.IsActionJustPressed("drop_item"))
		{
            weapon.DropWeapon();
        }
        if (Input.IsActionPressed("attack") && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			weapon._attack(delta);
		}
		else
		{
            weapon.shooting = false;
		}
        if (Input.IsActionJustPressed("interact") && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			player.interact();
		}
        if (Input.IsActionJustPressed("reload") && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			weapon._reload();
		}
        if (Input.IsActionJustPressed("buy_menu"))
        {
            Globals.PlayerUI.playerUI().buy_menu();
        }
    }

    public void update_debug()
    {
        //Globals.debug.update_debug_property("Position", player.Position);
        //Globals.debug.update_debug_property("Speed", speed);
        //Globals.debug.update_debug_property("FOV", player._camera.Fov);
    }
}
