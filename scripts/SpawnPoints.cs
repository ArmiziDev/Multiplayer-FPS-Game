using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnPoints : Node3D
{
	[Export] private Node3D FiveVFiveNode;
	[Export] private Node3D FreeForAllNode;
	[Export] private Node3D ZombiesNode;

	public List<Node3D> RedSpawnPoints = new List<Node3D>();
    public List<Node3D> BlueSpawnPoints = new List<Node3D>();
    public List<Node3D> FFASpawnPoints = new List<Node3D>();
    public List<Node3D> ZombiesSpawnPoints = new List<Node3D>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetupSpawnPoints(FFASpawnPoints, FreeForAllNode);
		SetupSpawnPoints(RedSpawnPoints, FiveVFiveNode.GetNode<Node3D>("RedTeam"));
        SetupSpawnPoints(RedSpawnPoints, FiveVFiveNode.GetNode<Node3D>("BlueTeam"));
		SetupSpawnPoints(ZombiesSpawnPoints, ZombiesNode);
    }

	public void SetupSpawnPoints(List<Node3D> spawn_list, Node3D node)
	{
        foreach (Node3D SpawnPoint in node.GetChildren())
        {
			spawn_list.Add(SpawnPoint);
        }
    }

	public Node3D GetFFASpawnPoint()
	{
		Node3D spawn_point = FFASpawnPoints[Mathf.RoundToInt(GD.RandRange(0, FFASpawnPoints.Count - 1))];

		return spawn_point;
	}
	public Node3D GetRedTeamSpawnPoint()
	{
        Node3D spawn_point = RedSpawnPoints[Mathf.RoundToInt(GD.RandRange(0, RedSpawnPoints.Count - 1))];

        return spawn_point;
    }
    public Node3D GetBlueTeamSpawnPoint()
    {
		Node3D spawn_point = BlueSpawnPoints[Mathf.RoundToInt(GD.RandRange(0, BlueSpawnPoints.Count - 1))];

        return spawn_point;
    }
}
