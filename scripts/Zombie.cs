using Godot;
using System;

public partial class Zombie : CharacterBody3D
{
	public Area3D EnemyDetection;
	public Player target;
	public const float Speed = 2.0f;

    public override void _Ready()
    {
        EnemyDetection = GetNode<Area3D>("%EnemyDetection");
    }

	public override void _PhysicsProcess(double delta)
		{
			if (target == null)
			{
				// No target, stay idle or perform other logic
				return;
			}
			
			Vector3 velocity = Velocity;

			// Calculate the direction to the target
			Vector3 direction = (target.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
			
			// Apply movement towards the target
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			// Apply gravity
			if (!IsOnFloor())
			{
				velocity.Y += GetGravity().Y * (float)delta;
			}
			else
			{
				velocity.Y = 0;
			}

			Velocity = velocity;

			// Move the zombie
			MoveAndSlide();
	}

	public void Die()
	{
		GD.Print("MEOW");
		QueueFree();
	}

	public void _on_enemy_detection_body_entered(Node3D body)
	{
		if (body is Player)
		{
			target = (Player)body;
		}
	}

	public void _on_enemy_detection_body_exited(Node3D body)
	{
		if (body == target)
		{
			target = null;
		}
	}

}
