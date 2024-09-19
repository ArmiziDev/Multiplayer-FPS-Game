using Godot;
using System;

public partial class SprintingPlayerState : PlayerMovementState
{
	[Export] public float TOP_ANIM_SPEED = 1.6f;
	[Export] public float WEAPON_BOB_SPEED = 7.0f;
	[Export] public float WEAPON_BOB_H = 2.3f;
	[Export] public float WEAPON_BOB_V = 1.2f;
	public override void Enter(PlayerMovementState previous_state)
    {
		state_machine.animationPlayer.Play("Sprinting", 0.5f, 1.0f);
		speed = player.sprint_speed;
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

		if(Input.IsActionJustReleased("sprint"))
		{
			state_machine.OnStateTransition("WalkingPlayerState");
		}

		if(Input.IsActionJustPressed("crouch") && player.Velocity.Length() > 6.0f)
		{
			state_machine.OnStateTransition("SlidingPlayerState");
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

    public void SetAnimationSpeed(float spd)
	{
		float minSpeed = 0.0f;  
		float maxSpeed = player.sprint_speed; 

		float alpha = Mathf.Clamp((spd - minSpeed) / (maxSpeed - minSpeed), 0.0f, 1.0f);

		alpha = Mathf.SmoothStep(0.0f, 1.0f, alpha);

		state_machine.animationPlayer.SpeedScale = Mathf.Lerp(0.0f, TOP_ANIM_SPEED, alpha);
	}
}
