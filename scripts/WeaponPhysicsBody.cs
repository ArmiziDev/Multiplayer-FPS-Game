using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class WeaponPhysicsBody : RigidBody3D
{
	public int weaponbody_id;

	[Export] public bool Physics = true;
	[Export] public Weapons weapon;
	[Export] public MeshInstance3D gun_mesh;

	// Pickup Signal
	[Signal] public delegate void WeaponPickupEventHandler(Weapons weapon);

	// Gun Info
	private int current_mag;

    public void InitializeWeaponPhysicsBody(WeaponControllerSingleMesh weapon_controller)
    {
		if (weapon != null)
		{
			LoadWeapon();
		}
		// Connect the WeaponPickup signal to the player's weapon controller
        if (weapon != null)
        {
			Connect(SignalName.WeaponPickup, new Callable(weapon_controller, nameof(weapon_controller.OnWeaponPickedUp)));
        }
        else
        {
            Globals.PlayerUI?.debug().debug_err("WEAPON is null. Cannot connect WeaponPickup signal.");
        }
    }

	public void Pickup()
	{
		Globals.PlayerUI.debug().debug_message("Picked up " + weapon.name + " Ammo: " + weapon.current_ammo + " / " + weapon.magazine_capacity);
		// Set Weapon Info
		weapon.current_ammo = current_mag;
		// Emit the signal before queueing the weapon for freeing
    	EmitSignal(SignalName.WeaponPickup, weapon);  // Emit the signal here

		if (Multiplayer.IsServer())
		{
			networkedWeaponPickup();
        }
		else
		{
			RpcId(1, nameof(networkedWeaponPickup));
		}
	}

    public void SetWeapon(Weapons _weapon, int weapon_id = 0)
	{
		weaponbody_id = weapon_id;
		weapon = _weapon;
		LoadWeapon();
	}

	private void LoadWeapon()
	{
		if (gun_mesh == null) Globals.PlayerUI?.debug().debug_err("Failed to Initialize Gun Mesh for WeaponPhysicsBody");
		if (weapon == null) Globals.PlayerUI?.debug().debug_err("Failed To Initialize Weapon in WeaponPhysicsBody");
		if (weapon.mesh == null) Globals.PlayerUI?.debug().debug_err("Failed To Initialize Weapon Mesh in WeaponPhysicsBody");
		gun_mesh.Mesh = weapon.mesh;
		gun_mesh.Scale = weapon.scale;
		current_mag = weapon.current_ammo;
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void networkedWeaponPickup()
	{
        if (Multiplayer.IsServer())
        {
            Rpc(nameof(networkedWeaponPickup));
        }
        QueueFree();
    }
}