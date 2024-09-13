using Godot;
using System;

public partial class PlayerInfo : Node
{
	public Team player_team;
	public int server_id;
	public int health;

	// stats
	public int kills;
	public int deaths;
	public int assists;
}
