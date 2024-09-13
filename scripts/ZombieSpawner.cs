using Godot;
using System;

public partial class ZombieSpawner : Node3D
{
	[Export] public float SpawnRate = 2.0f;
	[Export] public int SpawnAmountMin = 1;
	[Export] public int SpawnAmountMax = 4;
	[Export] public Vector3 SpawnRange = new Vector3(30.0f, 0.0f, 30.0f);
	[Export] public PackedScene ZombieScene;
	[Export] public bool isSpawning = false; // Track whether the spawner is active

	private Timer spawn_timer;
	private Random random = new Random();
	public override void _Ready()
	{
		spawn_timer = GetNode<Timer>("%SpawnTimer");
		spawn_timer.WaitTime = SpawnRate;

		SpawnZombies();
	}

	private void SpawnZombies()
    {
        if (!isSpawning) return; // Ensure zombies only spawn when the spawner is active

		//GD.Print("SpawningZombies");

        int spawnAmount = random.Next(SpawnAmountMin, SpawnAmountMax + 1);
        for (int i = 0; i < spawnAmount; i++)
        {
            Zombie zombie = ZombieScene.Instantiate<Zombie>();

            // Generate a random position within the spawn range
            Vector3 randomOffset = new Vector3(
                (float)(random.NextDouble() * 2 - 1) * SpawnRange.X,
                0.0f, // Assuming you don't want vertical variation, otherwise use `random.NextFloat() * SpawnRange.Y`
                (float)(random.NextDouble() * 2 - 1) * SpawnRange.Z
            );
            Vector3 spawnPosition = GlobalTransform.Origin + randomOffset;
            zombie.Position = spawnPosition;

            AddChild(zombie); // Add the zombie to the scene tree
        }
    }

	public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawn_timer.Start();
            GD.Print("Zombie spawning started.");
        }
    }

    public void StopSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            spawn_timer.Stop();
            GD.Print("Zombie spawning stopped.");
        }
    }
    public void ToggleSpawning()
    {
        if (isSpawning)
        {
            StopSpawning();
        }
        else
        {
            StartSpawning();
        }
    }
}
