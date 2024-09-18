using Godot;
using System;

public partial class WeaponRecoil : Node3D
{
    [Export] public WeaponControllerSingleMesh weapon;
    [Export] public bool Enabled = false;

    public Vector3 current_rotation;
    public Vector3 target_rotation;

    // Random reference
    private Random random;

    public override void _Ready()
    {
        // Will use for randomness for recoil
        random = new Random();
    }

    public override void _Process(double delta)
    {
        if (!weapon.upwards_recoil) // If CSGO type style is false then we use this
        {
            // Interpolate target rotation towards zero for smooth return to center
            target_rotation = target_rotation.Lerp(Vector3.Zero, weapon.WEAPON_TYPE.recoil_speed_rotation_camera * (float)delta);

            // Interpolate current rotation towards the target rotation for smooth recoil
            current_rotation = current_rotation.Lerp(target_rotation, weapon.WEAPON_TYPE.recoil_snap_amount_rotation_camera * (float)delta);

            // Apply the rotation to the node
            Rotation = current_rotation;
        }
    }

    public void add_recoil()
    {
        // Only adjust the Y-axis for straight up recoil
        target_rotation.X += weapon.WEAPON_TYPE.recoil_amount_rotation_camera.X;
		target_rotation.Y += GetRandomFloatInRange(-weapon.WEAPON_TYPE.recoil_amount_rotation_camera.Y, weapon.WEAPON_TYPE.recoil_amount_rotation_camera.Y);
    }

    private float GetRandomFloatInRange(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }
}
