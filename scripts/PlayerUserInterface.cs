using Godot;
using System;

public partial class PlayerUserInterface : Control
{
	[Export] public Reticle _reticle { get; set; }
	[Export] public Debug _debug { get; set; }
	[Export] public PlayerUI _playerUI { get; set; }

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