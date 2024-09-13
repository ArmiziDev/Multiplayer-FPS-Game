using Godot;
using System;

public partial class WeaponController : Node3D
{
	public WeaponResource CurrentWeapon { get; set; }

	// Dictionary to store preloaded weapons
    private Godot.Collections.Dictionary<string, WeaponResource> loadedWeapons = new Godot.Collections.Dictionary<string, WeaponResource>();
    private Godot.Collections.Array<WeaponResource> loadout = new Godot.Collections.Array<WeaponResource>();

	
	[Export]
	public WeaponResource WEAPON_TYPE
	{
		get => CurrentWeapon;
		set
		{
			CurrentWeapon = value;
			if(Engine.IsEditorHint())
			{
				LoadWeapon();
			}
		}
	}

	public MeshInstance3D mesh_arm { get; set; }
	public MeshInstance3D mesh_gun { get; set; }
	public MeshInstance3D mesh_barrel { get; set; }
	public MeshInstance3D mesh_mag { get; set; }

	public bool can_shoot = true;
	public bool reloading = false;
    private Vector2 mouse_movement;
	public PackedScene raycast_bullet_hole;

	public Timer shoot_timer;

	//Signals
    [Signal] public delegate void WeaponFiredEventHandler();

    public override void _Ready()
    {
		mesh_arm = GetNode<MeshInstance3D>("%ArmMesh");
		mesh_gun = GetNode<MeshInstance3D>("%GunMesh");
		mesh_barrel = GetNode<MeshInstance3D>("%BarrelMesh");
		mesh_mag = GetNode<MeshInstance3D>("%MagMesh");

		shoot_timer = GetNode<Timer>("%ShootTimer");

		PreloadWeapons();
    }

	private void PreloadWeapons()
	{
		loadedWeapons["pistol"] = (WeaponResource)GD.Load("res://meshes/weapons/fps_pistol/weapon_pistol.tres");
	}

	private void LoadWeapon()
	{
		if (WEAPON_TYPE != null)
		{
			can_shoot = false;

			Position = WEAPON_TYPE.position;
			RotationDegrees = WEAPON_TYPE.rotation;
			Scale = WEAPON_TYPE.scale;

			shoot_timer.WaitTime = WEAPON_TYPE.time_between_bullets;

			// ARM MESH WILL BE FIRST
			// GUN MESH IS SECOND
			// BARREL MESH IS THIRD
			// MAG MESH IS FOURTH
			mesh_arm.Mesh = WEAPON_TYPE.meshes[0];
			mesh_gun.Mesh = WEAPON_TYPE.meshes[1];
			mesh_barrel.Mesh = WEAPON_TYPE.meshes[2];
			mesh_mag.Mesh = WEAPON_TYPE.meshes[3];
		}
	}



}
