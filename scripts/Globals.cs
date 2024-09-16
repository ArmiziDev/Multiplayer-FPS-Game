using Godot;
using System;
using System.Collections.Generic;

public partial class Globals : Node
{
	public static PlayerInfo localPlayerInfo { get; set; }
	public static PlayerUserInterface PlayerUI {get; set;} // refrence for playerUI
	public static GameManager gameManager { get; set; }
	public static int game_mode { get; set; }
	public static List<PlayerInfo> PLAYERS { get; set; } = new List<PlayerInfo>();
	public static Godot.Collections.Dictionary<string, Weapons> weaponDictionary = new Godot.Collections.Dictionary<string, Weapons>();
}
