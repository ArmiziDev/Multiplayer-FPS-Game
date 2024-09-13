using Godot;
using System;

public partial class Reticle : CenterContainer
{
	[Export] public Godot.Collections.Array<Line2D> RETICLE_LINES = new Godot.Collections.Array<Line2D>();
	[Export] CharacterBody3D PLAYER_CONTROLLER;
	[Export] private float RETICLE_SPEED = 0.25f;
	[Export] private float RETICLE_DISTANCE = 2.0f;
	[Export] private float DOT_RADIUS = 1.0f;
	[Export] private Color DOT_COLOR = Colors.White;
	public override void _Ready()
	{
		QueueRedraw();
	}

    public override void _Process(double delta)
    {
        adjust_reticle_lines();
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero,DOT_RADIUS, DOT_COLOR);
    }

	private void adjust_reticle_lines()
	{
		float speed = Vector3.Zero.DistanceTo(PLAYER_CONTROLLER.GetRealVelocity());

		// Adjust Reticle Line Position
    	RETICLE_LINES[0].Position = RETICLE_LINES[0].Position.Lerp(Vector2.Zero + new Vector2(0, RETICLE_DISTANCE * -speed), RETICLE_SPEED); // TOP
		RETICLE_LINES[1].Position = RETICLE_LINES[1].Position.Lerp(Vector2.Zero + new Vector2(RETICLE_DISTANCE * speed, 0), RETICLE_SPEED); // RIGHT
    	RETICLE_LINES[2].Position = RETICLE_LINES[2].Position.Lerp(Vector2.Zero + new Vector2(0, RETICLE_DISTANCE * speed), RETICLE_SPEED); // BOTTOM
    	RETICLE_LINES[3].Position = RETICLE_LINES[3].Position.Lerp(Vector2.Zero + new Vector2(RETICLE_DISTANCE * -speed, 0), RETICLE_SPEED); // LEFT
	}
}
