using Godot;
using System;

public partial class RaycastBulletHole : Decal
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GpuParticles3D bulletScatter = GetNode<GpuParticles3D>("%BulletScatter");
        bulletScatter.Emitting = true;
        CreateBulletHole();
    }

    private async void CreateBulletHole()
    {
        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
        Tween fade = GetTree().CreateTween();
        fade.TweenProperty(this, "modulate:a", 0, 4.5);
        await ToSignal(GetTree().CreateTimer(4.5f), "timeout");
        QueueFree();
    }
}