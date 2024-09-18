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

    int current_redSpawnPointIndex = 0;
    int current_blueSpawnPointIndex = 0;
    int current_FFASpawnIndex = 0;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		SetupSpawnPoints(FFASpawnPoints, FreeForAllNode);
		SetupSpawnPoints(RedSpawnPoints, FiveVFiveNode.GetNode<Node3D>("%RedTeam"));
        SetupSpawnPoints(BlueSpawnPoints, FiveVFiveNode.GetNode<Node3D>("%BlueTeam"));
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
        if (FFASpawnPoints.Count == 0) return null;

        Node3D spawn_point = FFASpawnPoints[current_FFASpawnIndex];

        if (FFASpawnPoints.Count > current_FFASpawnIndex)
        {
            current_FFASpawnIndex++;
        }
        else
        {
            current_FFASpawnIndex = 0;
        }

        //Area3D player_detection = spawn_point.GetNode<Area3D>("CheckPlayer");

		return spawn_point;
	}

    public void resetTeamSpawnPoints()
    {
        current_blueSpawnPointIndex = 0;
        current_redSpawnPointIndex = 0;
    }

	public Node3D GetRedTeamSpawnPoint()
	{
		if (RedSpawnPoints.Count == 0) return null;
        Node3D spawn_point = RedSpawnPoints[current_redSpawnPointIndex];

        current_redSpawnPointIndex++;

        return spawn_point;
    }
    public Node3D GetBlueTeamSpawnPoint()
    {
        if (BlueSpawnPoints.Count == 0) return null;
        Node3D spawn_point = BlueSpawnPoints[current_blueSpawnPointIndex];

        current_blueSpawnPointIndex++;

        return spawn_point;
    }
}
