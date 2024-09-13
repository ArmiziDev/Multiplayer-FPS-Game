using Godot;
using System;

public partial class WalkingPlayerState : PlayerMovementState
{
	[Export] public float TOP_ANIM_SPEED = 2.2f;
	[Export] public float WEAPON_BOB_SPEED = 5.0f;
	[Export] public float WEAPON_BOB_H = 2.0f;
	[Export] public float WEAPON_BOB_V = 1.0f;
    public override void Enter(PlayerMovementState previous_state)
    {
        state_machine.animationPlayer.Play("Walking", -1.0f, 1.0f);
		speed = player.default_speed;
    }
    public override void Exit()
    {
		state_machine.animationPlayer.SpeedScale = 1.0f;
		state_machine.animationPlayer.Pause();
    }
    public override void Update(float delta)
	{
		base.Update(delta);

		weapon.sway_weapon(delta, false);
		weapon._weapon_bob(delta, WEAPON_BOB_SPEED, WEAPON_BOB_H, WEAPON_BOB_V);

		SetAnimationSpeed(player.Velocity.Length());

		if(Input.IsActionPressed("sprint") && player.IsOnFloor())
		{
			state_machine.OnStateTransition("SprintingPlayerState");
		}

		if (Input.IsActionJustPressed("crouch") && player.IsOnFloor())
		{
			state_machine.OnStateTransition("CrouchingPlayerState");
		}

		if (Input.IsActionJustPressed("jump") && player.IsOnFloor())
		{
			state_machine.OnStateTransition("JumpingPlayerState");
		}

		if (player.Velocity.Length() == 0)
		{
			state_machine.OnStateTransition("IdlePlayerState");
		}

		if (player.Velocity.Y < -3.0 && !player.IsOnFloor())
		{
			state_machine.OnStateTransition("FallingPlayerState");
		}
	}

	public void SetAnimationSpeed(float spd)
	{
		// Assuming spd is the player's current speed and we want to map it to a [0, 1] range.
		// player.default_speed is the base speed, 
		// and TOP_ANIM_SPEED is the maximum animation speed.

		float minSpeed = 0.0f;  // Minimum player speed (can be adjusted)
		float maxSpeed = player.default_speed;  // Maximum player speed

		// Remapping spd to a value between 0 and 1
		float alpha = Mathf.Clamp((spd - minSpeed) / (maxSpeed - minSpeed), 0.0f, 1.0f);

		// Smoothing the transition using a smoothstep function or another easing function if desired
		alpha = Mathf.SmoothStep(0.0f, 1.0f, alpha);

		// Setting the animation speed with a smoother interpolation
		state_machine.animationPlayer.SpeedScale = Mathf.Lerp(0.0f, TOP_ANIM_SPEED, alpha);
	}

}
