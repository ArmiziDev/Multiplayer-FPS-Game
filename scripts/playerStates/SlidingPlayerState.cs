using Godot;
using System;

public partial class SlidingPlayerState : PlayerMovementState
{
    [Export] public ShapeCast3D CROUCH_SHAPECAST;
    [Export] public float SLIDE_ANIMATION_SPEED = 4.0f;

    private Vector3 tilt = Vector3.Zero;
    private Timer slide_timer;

    public override void Enter(PlayerMovementState previous_state)
    {
        //Input.ActionRelease("sprint"); // release sprint key

        SetTilt(player.Rotation);
        //state_machine.animationPlayer.GetAnimation("Sliding").TrackSetKeyValue(4, 0, player.Velocity.Length());
        state_machine.animationPlayer.SpeedScale = 1.0f;
        state_machine.animationPlayer.Play("Sliding", -1.0, SLIDE_ANIMATION_SPEED);

        //Max Sprint Timer
        slide_timer = new Timer();
        slide_timer.OneShot = true;
        slide_timer.WaitTime = state_machine.animationPlayer.CurrentAnimationLength / SLIDE_ANIMATION_SPEED;
        AddChild(slide_timer);
        slide_timer.Timeout += () => 
        {
            Globals.debug.debug_message("Sprint Timer has finished!");
            Finish("CrouchingPlayerState");
        };
        slide_timer.Start();
    }

    public override void Update(float delta)
    {
        player.UpdateGravity(delta);
        player.UpdateVelocity();
        player.run_raycast();
        UpdateGeneralInputs(delta);

        Slide();

        update_debug();
        Globals.debug.update_debug_property("Slide Timer", Mathf.Round(slide_timer.TimeLeft));
    }

    private void Slide()
    {
        if (Input.IsActionJustReleased("crouch"))
        {
            Finish("CrouchingPlayerState");
        }
        if (Input.IsActionJustPressed("jump"))
        {
            Finish("JumpingPlayerState");
        }
    }

    private void SetTilt(Vector3 player_rotation)
    {
        tilt.Z = Mathf.Clamp(player_rotation.Z * player.slide_tilt, -0.1f, 0.1f);
        if (tilt.Z == 0.0f) tilt.Z = 0.05f;

        state_machine.animationPlayer.GetAnimation("Sliding").TrackSetKeyValue(3, 0, tilt); // track index 7 is camera rotation
        state_machine.animationPlayer.GetAnimation("Sliding").TrackSetKeyValue(3, 1, tilt);
    }
    
    public void Finish(StringName nextState)
    {
        slide_timer.Stop(); //stop timer
        Globals.debug.debug_message("Finished Slide");
        state_machine.animationPlayer.CurrentAnimation = "Sliding";
        state_machine.animationPlayer.Seek(state_machine.animationPlayer.CurrentAnimationLength, true); // skip to end of animation

        state_machine.OnStateTransition(nextState);
    }
}