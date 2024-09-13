using Godot;
using System;
using System.Collections.Generic;

public partial class Globals : Node
{
	public static PlayerInfo localPlayerInfo;
	public static Debug debug {get; set;} // refrence for debugpanel for debug property assignments
	public static List<PlayerInfo> PLAYERS { get; set; } = new List<PlayerInfo>();
	public static Godot.Collections.Dictionary<string, Weapons> weaponDictionary = new Godot.Collections.Dictionary<string, Weapons>();

}
