using Godot;
using System;
using System.Transactions;

public partial class JumpingPlayerState : PlayerMovementState
{
	public int current_jumps;
	public int jump_limit = 2; //only allowed to jump twice without touching floor
	public override void Enter(PlayerMovementState previous_state)
    {
		state_machine.animationPlayer.Play("JumpStart");

		Jump();

		input_multiplier = player.Jump_Input_Multiplier; //slow down movement inputs

		current_jumps = 1; // we are currently on the first jump

		// Speed Boost if Jumped From Slide
		if (previous_state.Name == "SlidingPlayerState")
		{
			SlideJump();
		}
    }
    public override void Update(float delta)
    {
        base.Update(delta);

		if (Input.IsActionJustPressed("jump") && current_jumps < jump_limit)
		{
			current_jumps ++;
			AirJump();
		}

		if (player.IsOnFloor())
		{
			state_machine.OnStateTransition("IdlePlayerState");
		}
    }
    public override void Exit()
    {
		state_machine.animationPlayer.Play("JumpEnd");
		input_multiplier = 1.0f; //resseting input multiplier
		state_machine.animationPlayer.SpeedScale = 1.0f;

		deceleration = player.DECELERATION;
		acceleration = player.ACCELERATION;
    }

	public void SlideJump()
	{
		speed = 9.0f;
		//deceleration = 0f;
		acceleration *= 3f;
	}

	public void Jump()
	{
		Vector3 jump_velocity = new Vector3(0.0f, player.JumpVelocity, 0.0f);  // Get a copy of the current velocity
		player.Velocity += jump_velocity;  // Assign the modified velocity back to the player
	}
	public void AirJump()
	{
		Vector3 jump_velocity = new Vector3(0.0f, player.JumpVelocity, 0.0f);
		player.Velocity = jump_velocity;  
	}
}
