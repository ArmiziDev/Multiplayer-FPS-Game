using Godot;
using System;

public partial class BulletLineCast : Node3D
{
    [Export] public float speed = 200.0f;  // Speed at which the bullet moves
    public Vector3 startPosition;  // Bullet's starting position
    public Vector3 targetPosition;  // Bullet's ending (target) position

    private Vector3 direction;  // The direction from start to end
    private float totalDistance;  // Total distance to travel

    public void SetTarget(Vector3 start, Vector3 target)
    {
        // Set the start and target positions
        startPosition = start;
        targetPosition = target;

        // Calculate the direction and the total distance to travel
        direction = (targetPosition - startPosition).Normalized();
        totalDistance = startPosition.DistanceTo(targetPosition);

        // Set the bullet's initial position
        Position = startPosition;

        // Rotate the bullet to face the target
        LookAt(targetPosition, Vector3.Up);  // Make the bullet face the targetPosition
    }

    public override void _Process(double delta)
    {
        // Move the bullet towards the target
        Vector3 movement = direction * speed * (float)delta;
        Position += movement;

        // Check if the bullet has reached the target or passed it
        if (Position.DistanceTo(startPosition) >= totalDistance)
        {
            QueueFree();  // Remove the bullet when it reaches the target
        }
    }

    public void _on_area_3d_body_entered(Node3D node)
    {
        // Optionally free the bullet on collision
        Globals.PlayerUI.debug().debug_message("Linecast Entered body: " + node.Name);
        //QueueFree();
    }
}
