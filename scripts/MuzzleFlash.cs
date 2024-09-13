using Godot;
using System;

public partial class MuzzleFlash : Node3D
{
	public WeaponControllerSingleMesh weapon;
	[Export] public float flash_time { get; set; } = 0.05f;
	[Export] public OmniLight3D light;
	[Export] public GpuParticles3D emitter;
	public void InitializeMuzzleFlash(WeaponControllerSingleMesh weapon)
	{
		this.weapon = weapon;
		light.Visible = false;
		emitter.Emitting = false;
	}

	public async void add_muzzle_flash()
	{
		light.Visible = true;
		emitter.Emitting = true;
		await ToSignal(GetTree().CreateTimer(flash_time), "timeout");

		light.Visible = false;
	}
}
