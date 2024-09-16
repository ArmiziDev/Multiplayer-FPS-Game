using Godot;
using System;

public partial class PlayerKillUi : Panel
{
	// Called when the node enters the scene tree for the first time.
	private Label Killer;
	private Label Killed;

	private Timer displayTimer;

	public override void _Ready()
	{
		Killer = GetNode<Label>("%Killer");
		Killed = GetNode<Label>("%Killed");

		displayTimer = GetNode<Timer>("%DisplayTime");
    }

	public void SetPlayerKillUI(PlayerInfo killer, PlayerInfo killed)
	{
		displayTimer.Start();
		Killer.Text = killer.Name;
		Killer.LabelSettings.FontColor = new Color(1, 0, 0);

        Killed.Text = killed.Name;

		switch (killer.player_team)
		{
			case Team.Red:
                Killer.LabelSettings.FontColor = new Color(1, 0, 0);
                break;
			case Team.Blue:
                Killer.LabelSettings.FontColor = new Color(0, 0, 1);
                break;
			case Team.None:
                Killer.LabelSettings.FontColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
                break;
		}
        switch (killed.player_team)
        {
            case Team.Red:
                Killed.LabelSettings.FontColor = new Color(1, 0, 0);
                break;
            case Team.Blue:
                Killed.LabelSettings.FontColor = new Color(0, 0, 1);
                break;
            case Team.None:
                Killed.LabelSettings.FontColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
                break;
        }
    }

	public void _on_display_time_timeout()
	{
		QueueFree();
	}

}
