using Godot;
using System;

public partial class WeaponViewport : SubViewport
{
	public Vector2 screen_size;
	public override void _Ready()
	{
		screen_size = GetWindow().Size;
		Size = (Vector2I)screen_size; // setting viewport to window size
	}
}
