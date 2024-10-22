using Godot;
using System;

public partial class PlayerUserInterface : Control
{
	[Export] public Reticle _reticle { get; set; }
	[Export] public Debug _debug { get; set; }
	[Export] public PlayerUI _playerUI { get; set; }

    public override void _Ready()
    {
        Globals.PlayerUI = this;

		// Debugging
        debug().add_debug_property("Velocity", Globals.localPlayer.Velocity.Length());
    }

    public void update_debug_property(string title, object value)
	{
		_debug.update_debug_property(title, value);
    }

	public Reticle reticle()
	{
		return _reticle;
	}

	public PlayerUI playerUI()
	{
		return _playerUI;
	}

	public Debug debug()
	{
		return _debug;
	}

}