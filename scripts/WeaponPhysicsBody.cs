using Godot;
using System;

public partial class WeaponPhysicsBody : RigidBody3D
{
	[Export] public bool Physics = true;
	[Export] public Weapons weapon;
	public MeshInstance3D gun_mesh;

	// Pickup Signal
	[Signal] public delegate void WeaponPickupEventHandler(Weapons weapon);

	// Gun Info
	private int current_mag;

    public void InitializeWeaponPhysicsBody(WeaponControllerSingleMesh weapon_controller)
    {
        gun_mesh = GetNode<MeshInstance3D>("%GunMesh");
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
            Globals.debug.debug_err("WEAPON is null. Cannot connect WeaponPickup signal.");
        }
    }

	public void Pickup()
	{
		// Set Weapon Info
		weapon.current_ammo = current_mag;
		// Emit the signal before queueing the weapon for freeing
    	EmitSignal(SignalName.WeaponPickup, weapon);  // Emit the signal here
		
		QueueFree();
		//return weapon;
	}

    public void SetWeapon(Weapons _weapon)
	{
		weapon = _weapon;
		LoadWeapon();
	}

	private void LoadWeapon()
	{
		if (gun_mesh == null) GD.PrintErr("Failed to Initialize Gun Mesh for WeaponPhysicsBody");
		if (weapon == null) GD.PrintErr("Failed To Initialize Weapon in WeaponPhysicsBody");
		if (weapon.mesh == null) GD.PrintErr("Failed To Initialize Weapon Mesh in WeaponPhysicsBody");
		gun_mesh.Mesh = weapon.mesh;
		gun_mesh.Scale = weapon.scale;

		current_mag = weapon.current_ammo;
	}

}
