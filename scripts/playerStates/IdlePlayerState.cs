using Godot;
using System;

public partial class IdlePlayerState : PlayerMovementState
{
    public override void Enter(PlayerMovementState previous_state)
    {
		speed = player.default_speed;
		acceleration = player.ACCELERATION;
		deceleration = player.DECELERATION;
    }

    public override void Exit()
    {
        state_machine.animationPlayer.SpeedScale = 1.0f;
    }
    public override void Update(float delta)
	{
		base.Update(delta);

		weapon.sway_weapon(delta, true);

		// Inputs

		if (Input.IsActionJustPressed("crouch") && player.IsOnFloor())
		{
			state_machine.OnStateTransition("CrouchingPlayerState");
		}

		if (player.Velocity.Length() > 0 && player.IsOnFloor())
		{
			state_machine.OnStateTransition("WalkingPlayerState");
		}

		if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
		{
			state_machine.OnStateTransition("JumpingPlayerState");
		}

		if (player.Velocity.Y < -3.0 && !player.IsOnFloor())
		{
			state_machine.OnStateTransition("FallingPlayerState");
		}
	}
}
