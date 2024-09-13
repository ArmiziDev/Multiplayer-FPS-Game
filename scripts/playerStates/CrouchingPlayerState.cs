using Godot;
using System;

public partial class CrouchingPlayerState : PlayerMovementState
{
    [Export] public float CROUCH_ANIMATION_SPEED = 4.0f;
    [Export] public float WEAPON_BOB_SPEED = 2.0f;
	[Export] public float WEAPON_BOB_H = 3.0f;
	[Export] public float WEAPON_BOB_V = 2.0f;
    [Export] public ShapeCast3D CROUCH_SHAPECAST;
    
    public bool RELEASED = false;

    public override void Enter(PlayerMovementState previous_state)
    {
        speed = player.crouch_speed;
        
        state_machine.animationPlayer.SpeedScale = 1.0f;
        if (previous_state.Name != "SlidingPlayerState")
        {
            state_machine.animationPlayer.Play("Crouching", -1.0f, CROUCH_ANIMATION_SPEED, false);
            speed = player.crouch_speed;
        }
        else if (previous_state.Name == "SlidingPlayerState")
        {
            //GD.Print("Crouching From SlidingState");
            // we want to stay crouched after sliding if something is above us we cant get up
            state_machine.animationPlayer.CurrentAnimation = "Crouching";
            state_machine.animationPlayer.Seek(state_machine.animationPlayer.CurrentAnimationLength, true); // skip to end of animation
        }
    }

    public override void Exit()
    {
        RELEASED = false;
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        weapon.sway_weapon(delta, false);
        weapon._weapon_bob(delta, WEAPON_BOB_SPEED, WEAPON_BOB_H, WEAPON_BOB_V);

        if (Input.IsActionJustReleased("crouch"))
        {
            Uncrouch("WalkingPlayerState");
        }
        else if (!Input.IsActionPressed("crouch") && !RELEASED)
        {
            RELEASED = true;
            Uncrouch("WalkingPlayerState");
        }

        if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
		{
            Uncrouch("JumpingPlayerState");
		}

        if (player.Velocity.Y < -3.0 && !player.IsOnFloor())
		{
			//Uncrouch("FallingPlayerState");
		}
    }

    private async void Uncrouch(StringName nextState)
    {
        //GD.Print("Uncrouching");
        if (!CROUCH_SHAPECAST.IsColliding() && 
            !Input.IsActionPressed("crouch") || nextState == "JumpingPlayerState")
        {
            //GD.Print("No Collisions Detected");
            // Play the uncrouch animation
            state_machine.animationPlayer.Play("Crouching", -1.0f, -CROUCH_ANIMATION_SPEED * 1.5f, true);

            // Await the animation_finished signal
            await ToSignal(state_machine.animationPlayer, "animation_finished");

            // Once the animation is finished, transition to the IdlePlayerState
            state_machine.OnStateTransition(nextState);
        }
        else if (CROUCH_SHAPECAST.IsColliding())
        {
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            Uncrouch("WalkingPlayerState");
        }
    }
}
