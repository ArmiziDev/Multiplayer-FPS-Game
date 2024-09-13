using Godot;
using System;

public partial class WeaponRecoilPhysics : Node3D
{
	public WeaponControllerSingleMesh weapon;
	private Vector3 current_position;
	private Vector3 target_position;
	private Random random;

	public void InitializeWeaponRecoilPhysics(WeaponControllerSingleMesh weapon)
	{
		this.weapon = weapon;
        // Will use for randomness for recoil
        random = new Random();
	}

	public override void _Process(double delta)
	{
		target_position = target_position.Lerp(Vector3.Zero, weapon.WEAPON_TYPE.speed_kickback * (float)delta);
		current_position = current_position.Lerp(target_position, weapon.WEAPON_TYPE.snap_amount_kickback * (float)delta);
		Position = current_position;
	}

	public void add_recoil()
    {
        // Only adjust the Y-axis for straight up recoil
        target_position.X += GetRandomFloatInRange(-weapon.WEAPON_TYPE.recoil_amount_kickback.X, weapon.WEAPON_TYPE.recoil_amount_kickback.X);
		target_position.Y += GetRandomFloatInRange(-weapon.WEAPON_TYPE.recoil_amount_kickback.Y, weapon.WEAPON_TYPE.recoil_amount_kickback.Y);
		target_position.Z -= GetRandomFloatInRange(weapon.WEAPON_TYPE.recoil_amount_kickback.Z, weapon.WEAPON_TYPE.recoil_amount_kickback.Z * 2.0f);
    }

    private float GetRandomFloatInRange(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }
}
