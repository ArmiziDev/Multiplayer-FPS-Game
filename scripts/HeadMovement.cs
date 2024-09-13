using Godot;
using System;

public partial class HeadMovement : Node3D
{
    [Export] public bool Enabled = true;
    [Export] public Player player;
    
    // Rotation sway settings
	[ExportCategory("Rotation Sway Settings")]
    [Export] public float rotationSwayAmount = 20.0f;    
    [Export] public float rotationSwaySpeed = 2.0f;     
    [Export] public float maxRotationSwayAngle = 0.8f; 
    [Export] public float rotationReturnSpeed = 3.0f;   

    // Position sway settings
	[ExportCategory("Position Sway Settings")]
    [Export] public float positionSwayAmount = 5.0f;   
    [Export] public float positionSwaySpeed = 2.0f;     
    [Export] public float maxPositionSwayOffset = 4f;
    [Export] public float positionReturnSpeed = 3.0f;   

    private Vector3 target_rotation;
    private Vector3 target_position;

    private float previousPlayerRotationY;  
    private Vector3 previousPlayerPosition;

    public override void _Ready()
    {
        // Initialize previous positions
        previousPlayerPosition = player.GlobalTransform.Origin;
    }

    public override void _Process(double delta)
    {
        if (!Enabled || player == null)
            return;

        // Adjust based on rotation
        AdjustBasedOnRotation(delta);

        // Adjust based on position
        //AdjustBasedOnPosition(delta); //Doesn't work because it is relative to where the mouse is pointing

        // Apply the calculated target rotation and position to the camera
        Rotation = Rotation.Lerp(target_rotation, rotationSwaySpeed * (float)delta);
        //Position = Position.Lerp(target_position, positionSwaySpeed * (float)delta);

        // Debugging (optional)
        Globals.debug.update_debug_property("HeadMovement Rotation", Rotation);
		Globals.debug.update_debug_property("Target Head Rotation", target_rotation);
    }

    private void AdjustBasedOnRotation(double delta)
    {
        // Calculate the offset (difference) between the current and previous rotation
        float rotationOffset = player._rotation.Y - previousPlayerRotationY;
		Globals.debug.update_debug_property("Head Rotation Offset", rotationOffset);

        // Calculate the sway based on the offset
        float sway = rotationOffset * rotationSwayAmount;

        // Apply the sway to the target rotation, clamping it to prevent excessive tilt
        target_rotation.Z = Mathf.Clamp(sway, -maxRotationSwayAngle, maxRotationSwayAngle);

        // If there's no significant rotation change, gradually return to center
        if (Mathf.Abs(rotationOffset) < Mathf.Epsilon)
        {
            target_rotation = target_rotation.Lerp(Vector3.Zero, rotationReturnSpeed * (float)delta);
        }

        // Update the previous rotation for the next frame
        previousPlayerRotationY = player._rotation.Y;
    }

    private void AdjustBasedOnPosition(double delta)
    {
        // Calculate the offset (difference) between the current and previous position
        Vector3 positionOffset = player.GlobalTransform.Origin - previousPlayerPosition;
		Globals.debug.update_debug_property("Head Position Offset", positionOffset.X);

        // Calculate the sway based on the position offset
        target_rotation.Z = Mathf.Clamp(-positionOffset.X * positionSwayAmount, -maxPositionSwayOffset, maxPositionSwayOffset);

        // If the player stops moving, gradually return to center
		/*
        if (positionOffset.Length() < Mathf.Epsilon)
        {
            target_rotation = target_position.Lerp(Vector3.Zero, positionReturnSpeed * (float)delta);
        }
		*/

        // Update the previous position for the next frame
        previousPlayerPosition = player.GlobalTransform.Origin;
    }
}
