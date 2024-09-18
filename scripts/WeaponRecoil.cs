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
        // Interpolate target rotation towards zero for smooth return to center
        //target_rotation = target_rotation.Lerp(Vector3.Zero, weapon.WEAPON_TYPE.speed_rotation * (float)delta);
        
        // Interpolate current rotation towards the target rotation for smooth recoil
        //current_rotation = current_rotation.Lerp(target_rotation, weapon.WEAPON_TYPE.snap_amount_rotation * (float)delta);
        
        // Apply the rotation to the node
        //Rotation = current_rotation;
    }

    public void add_recoil()
    {
        // Only adjust the Y-axis for straight up recoil
        target_rotation.X += weapon.WEAPON_TYPE.recoil_amount_rotation.X;
		target_rotation.Y += GetRandomFloatInRange(-weapon.WEAPON_TYPE.recoil_amount_rotation.Y, weapon.WEAPON_TYPE.recoil_amount_rotation.Y);
    }

    private float GetRandomFloatInRange(float min, float max)
    {
        return (float)(min + random.NextDouble() * (max - min));
    }
}
