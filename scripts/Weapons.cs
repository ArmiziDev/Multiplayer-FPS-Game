using Godot;
using System;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(Weapons),"", nameof(Resource))]
public partial class Weapons : Resource
{
    [Export] public StringName name { get; set; }
	public enum WeaponType { Empty, Gun, Melee, Utility }
	[Export] public WeaponType weapon_type { get; set; } = WeaponType.Gun;
	public enum GunClass { None, Rifle, Sniper, Shotgun, SMG, Pistol }
	[Export] public GunClass gun_class { get; set; } = GunClass.None;

	[ExportCategory("Weapon Orientation")]
	[Export] public Vector3 position { get; set; } = new Vector3(-0.2f, 0.2f, 1.5f);
	[Export] public Vector3 rotation { get; set; } = new Vector3(0f, -175f, 0f);
	[Export] public Vector3 scale { get; set; } = Vector3.One;
	
	[ExportCategory("Weapon Settings")]
	[Export] public float base_damage { get; set; } = 13.0f;
	[Export] public int current_ammo { get; set; } = 30;
	[Export] public int magazine_capacity { get; set; } = 30;
	[Export] public float reload_time { get; set; } = 1.5f;
	[Export] public float time_between_bullets { get; set; } = 2.0f;
	[Export] public bool automatic_reload { get; set; } = true;
	[Export] public float pullout_time { get; set; } = 1.5f;
	[Export] public int money_kill_reward { get; set; } = 100;
	[Export] public int cost { get; set; }


	public enum FireMode { SemiAuto, Burst, FullAuto}
	[Export] public FireMode fire_mode { get; set; } = FireMode.FullAuto;
	[Export(PropertyHint.Range, "1,5,1")] public int burst_fire_count { get; set; } = 3;
	[Export(PropertyHint.Range, "0,10,0.1")] public float bullet_spread { get; set; } = 1.0f;

	[ExportCategory("Weapon Recoil Kickback")]
	[Export] public Vector3 recoil_amount_kickback;
    [Export] public float snap_amount_kickback;
    [Export] public float speed_kickback;

	[ExportCategory("Weapon Recoil Rotation")]
	[Export] public Vector3 recoil_amount_rotation;
	[Export] public float recoil_follow_speed;
    [Export] public float recoil_reset_speed;
	[Export] public float recoil_reset_speed_amplifier = 2.0f;

	[ExportCategory("Weapon Sway")]
	[Export] public Vector2 sway_min { get; set; } = new Vector2(-20.0f, -20.0f); 
	[Export] public Vector2 sway_max { get; set; } = new Vector2(20.0f, 20.0f); 
	[Export(PropertyHint.Range, "0,0.2,0.01")] public float sway_speed_position { get; set; } = 0.07f;
	[Export(PropertyHint.Range, "0,0.2,0.01")] public float sway_speed_rotation { get; set; } = 0.1f;
	[Export(PropertyHint.Range, "0,0.25,0.01")] public float sway_amount_position { get; set; } = 0.1f;
	[Export(PropertyHint.Range, "0,50,0.1")] public float sway_amount_rotation { get; set; } = 30.0f;
	[Export] public float idle_sway_adjustment {get; set;} = 10.0f;
	[Export] public float idle_sway_rotation_strength { get; set; } = 300.0f;
	[Export(PropertyHint.Range, "0.1,10,0.1")] public float random_sway_amount { get; set; } = 5.0f;

	[ExportCategory("Visual Settings")]
	[Export] public Mesh mesh { get; set; }
	[Export] public bool shadow { get; set; }
	[Export] public Vector3 muzzle_flash_position { get; set; } = new Vector3(0.11f, 31.25f, 94.597f);

	[ExportCategory("Audio Settings")]
    [Export] public NodePath FireSoundPath { get; set; }
    [Export] public NodePath ReloadSoundPath { get; set; }
	public AudioStreamPlayer3D FireSound { get; set; }
	public AudioStreamPlayer3D ReloadSound { get; set; }

}
