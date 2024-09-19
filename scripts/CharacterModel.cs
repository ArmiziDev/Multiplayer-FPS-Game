using Godot;
using System;

public partial class CharacterModel : Node3D
{
	public AnimationTree animationTree;

	[Export] public float run = 0.0f;
	[Export] public float crouch = 0.0f;
	[Export] public float slide = 0.0f;
	[Export] public float blend_speed = 15.0f;

	[Export] public PlayerAnimation current_animation = PlayerAnimation.Idle;

	public override void _Ready()
	{
        animationTree = GetNode<AnimationTree>("AnimationTree");
	}

    public override void _PhysicsProcess(double delta)
    {
        handle_animations((float)delta);
    }

    public void handle_animations(float delta)
	{
		switch (current_animation)
		{ 
			case PlayerAnimation.Idle:
				run = Mathf.Lerp(run, 0.0f, blend_speed * delta);
                crouch = Mathf.Lerp(crouch, 0.0f, blend_speed * delta);
                slide = Mathf.Lerp(slide, 0.0f, blend_speed * delta);
                break;
			case PlayerAnimation.Running:
                run = Mathf.Lerp(run, 1.0f, blend_speed * delta);
                crouch = Mathf.Lerp(crouch, 0.0f, blend_speed * delta);
                slide = Mathf.Lerp(slide, 0.0f, blend_speed * delta);
                break;
			case PlayerAnimation.Crouch:
                run = Mathf.Lerp(run, 0.0f, blend_speed * delta);
                crouch = Mathf.Lerp(crouch, 1.0f, blend_speed * delta);
                slide = Mathf.Lerp(slide, 0.0f, blend_speed * delta);
                break;
		}

        UpdateTree();
    }

	public void UpdateTree()
	{
		animationTree.Set("parameters/Crouch/blend_amount", crouch);
        animationTree.Set("parameters/Run/blend_amount", run);
    }
}
