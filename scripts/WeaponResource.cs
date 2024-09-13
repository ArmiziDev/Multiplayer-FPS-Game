using Godot;
using System;
using MonoCustomResourceRegistry;

[Tool]
[RegisteredType(nameof(WeaponResource),"", nameof(Resource))]
public partial class WeaponResource : Resource
{
    [Export] public StringName name { get; set; }
	public enum WeaponType { Gun, Melee, Utility }
	[Export] public WeaponType weapon_type { get; set; } = WeaponType.Gun;

	[ExportCategory("Weapon Orientation")]
	[Export] public Vector3 position { get; set; }
	[Export] public Vector3 rotation { get; set; }
	[Export] public Vector3 scale { get; set; } = Vector3.One;
	
	[ExportCategory("Weapon Settings")]
	[Export] public float base_damage { get; set; } = 13.0f;
	[Export] public int current_ammo { get; set; } = 30;
	[Export] public int magazine_capacity { get; set; } = 30;
	[Export] public float reload_time { get; set; } = 1.5f;
	[Export] public float time_between_bullets { get; set; } = 2.0f;
	[Export] public bool automatic_reload { get; set; } = true;
	[Export] public float pullout_time { get; set; } = 1.5f;

	public enum FireMode { SemiAuto, Burst, FullAuto}
	[Export] public FireMode fire_mode { get; set; } = FireMode.FullAuto;
	[Export(PropertyHint.Range, "1,5,1")] public int burst_fire_count { get; set; } = 3;
	[Export(PropertyHint.Range, "0,10,0.1")] public float bullet_spread { get; set; } = 1.0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float aiming_accuracy_multiplier { get; set; } = 0.5f;

	[ExportCategory("Weapon Recoil Kickback")]
	[Export] public Vector3 recoil_amount_kickback;
    [Export] public float snap_amount_kickback;
    [Export] public float speed_kickback;

	[ExportCategory("Weapon Recoil Rotation")]
	[Export] public Vector3 recoil_amount_rotation;
    [Export] public float snap_amount_rotation;
    [Export] public float speed_rotation;

	[ExportCategory("Visual Settings")]
    [Export] public Mesh[] meshes = new Mesh[5];
	[Export] public bool shadow { get; set; }

	[ExportCategory("Animation Settings")]
	[Export] Animation animation_fire { get; set; }
	[Export] Animation animation_idle { get; set; }
	[Export] Animation animation_reload { get; set; }

	[ExportCategory("Audio Settings")]
    [Export] public NodePath FireSoundPath { get; set; }
    [Export] public NodePath ReloadSoundPath { get; set; }
	public AudioStreamPlayer3D FireSound { get; set; }
	public AudioStreamPlayer3D ReloadSound { get; set; }

}
