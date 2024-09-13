using Godot;
using System;

public partial class WeaponCamera : Camera3D
{
	[Export] Node3D MAIN_CAMERA;

    public override void _Process(double delta)
    {
        GlobalTransform = MAIN_CAMERA.GlobalTransform;
    }
}
