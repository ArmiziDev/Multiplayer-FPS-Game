using Godot;
using System;

public partial class FallingPlayerState : PlayerMovementState
{
	public int current_jumps;
	public int jump_limit = 0; // Not allowing air jumps from falling state as of right now
    public override void Enter(PlayerMovementState previous_state)
    {
        state_machine.animationPlayer.Pause();
		current_jumps = 0;
    }

    public override void Exit()
    {
        current_jumps = 0;
    }

    public override void Update(float delta)
    {
        base.Update(delta);

		if (Input.IsActionJustPressed("jump") && current_jumps < jump_limit)
		{
			current_jumps ++;
			AirJump();
		}

		if(player.IsOnFloor())
		{
			state_machine.animationPlayer.Play("JumpEnd");
			state_machine.OnStateTransition("IdlePlayerState");
		}
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
