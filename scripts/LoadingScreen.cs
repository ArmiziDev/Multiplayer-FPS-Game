using Godot;
using System;

public partial class LoadingScreen : Control
{
	[Export] private PackedScene playerScene;
	private string path;
	private bool loading;

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Process(double delta)
	{
		if (loading)
		{
			var progress = new Godot.Collections.Array();
			var status = ResourceLoader.LoadThreadedGetStatus(path, progress);
			if (status == ResourceLoader.ThreadLoadStatus.InProgress)
			{
				GetNode<ProgressBar>("%ProgressBar").Value = (double)progress[0] * 100;
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				SetProcess(false);
				GetNode<ProgressBar>("%ProgressBar").Value = 100;

				ChangeScene(ResourceLoader.LoadThreadedGet(path) as PackedScene);
			}
		}
	}

	public void AddPlayersToWorld()
	{
		foreach (var item in Globals.PLAYERS)
        {
            Player currentPlayer = playerScene.Instantiate<Player>();

            currentPlayer.Name = item.server_id.ToString();
            AddChild(currentPlayer);
        }
	}

	public void ChangeScene(PackedScene resource)
	{
		var rootNode = GetTree().Root;

		foreach (var item in GetTree().Root.GetChildren())
		{
			if (item is Node3D || item is Node2D || item is Control)
			{
				GetTree().Root.RemoveChild(item);
				item.QueueFree();
			}
		}
		Node currentNode = resource.Instantiate();
		rootNode.AddChild(currentNode);

		QueueFree();
	}

	public void LoadLevel(String _path)
	{
		this.path = _path;
		Show();
		if(ResourceLoader.HasCached(path))
		{
			ResourceLoader.LoadThreadedGet(path);
		}
		else
		{
			ResourceLoader.LoadThreadedRequest(path);
			loading = true;
		}
	}
}
