using Godot;
using System;

public partial class BulletLineCast : Node3D
{
	[Export] public float max_distance = 1000;
	[Export] public float speed = 100.0f;

	public float current_distance_traveled = 0;

	public override void _Process(double delta)
	{
		Vector3 new_position = Position;
		new_position.Z += speed * (float)delta;
		current_distance_traveled += speed * (float)delta;

		Position = new_position;

		if (current_distance_traveled > max_distance)
		{
			QueueFree();
		}
	}

	public void _on_area_3d_body_entered(Node3D node)
	{
		//QueueFree();
	}
}
