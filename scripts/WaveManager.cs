using Godot;
using Godot.Collections;

public partial class WaveManager : Node
{
    private int currentWave = 0;
    public int CurrentWave => currentWave;
    private int enemiesPerWave = 0;
    private int enemiesSpawned = 0;
    private float spawnInterval = 0.5f;
    private float spawnTimer = 0.0f;
    private float countdownTimer = 0.0f;
    private bool countdownActive = false;
    private float waveBreakTimer = 0.0f;
    private bool waveBreakActive = false;
    private bool waveSystemActive = false;

    private const float WaveBreakTime = 10.0f;

    private const float WaveCountdown = 10.0f;

    private Grid grid;

    public override void _Ready()
    {
        grid = GetParent().GetNode<Grid>("Grid");

    }

    public void StartWaveCountdown()
    {
        waveSystemActive = true;

        countdownActive = true;
        countdownTimer = 10.0f;

        GD.Print("First wave starts in 10 seconds!");
    }

    public void StartNextWave()
    {
        currentWave++;
        enemiesPerWave = 10 + (currentWave - 1) * 5;
        enemiesSpawned = 0;
        spawnTimer = 0.0f;
        GD.Print($"==== Wave {currentWave} STARTED ====");
        GD.Print($" Enemies to spawn: {enemiesPerWave}");

    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        //Czekamy az gracz postawi HQ
        if (!waveSystemActive)
            return;


        // Odliczanie do pierwszej fali
        if (countdownActive)
        {
            countdownTimer -= dt;

            if (countdownTimer <= 0)
            {
                countdownActive = false;
                StartNextWave();
            }

            return;
        }
        // Przerwa pomiędzy falami
        if (waveBreakActive)
        {
            waveBreakTimer -= dt;

            if (waveBreakTimer <= 0)
            {
                waveBreakActive = false;
                StartNextWave();
            }

            return;
        }
        // Wszystkie przeciwniki z aktualnej fali zostały zrespione
        if (enemiesSpawned >= enemiesPerWave)
        {
            waveBreakActive = true;
            waveBreakTimer = WaveBreakTime;

            GD.Print(
                $"Wave {currentWave} completed!"
            );

            GD.Print(
                $"Next wave in {WaveBreakTime} seconds..."
            );

            return;
        }

        //spawn przeciwnikow
        if (enemiesSpawned >= enemiesPerWave)
        {
            waveBreakActive = true;
            waveBreakTimer = WaveBreakTime;

            GD.Print(
                $"Wave {currentWave} completed!"
            );

            GD.Print(
                $"Next wave in {WaveBreakTime} seconds..."
            );

            return;
        }

        spawnTimer -= dt;

        if (spawnTimer <= 0)
        {
            SpawnEnemy();

            enemiesSpawned++;

            spawnTimer = spawnInterval;
        }
    }
    
    public void SpawnEnemy()
    {
        PackedScene enemyScene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
        Enemy enemy =
            enemyScene.Instantiate<Enemy>();

        GetParent().AddChild(enemy);

        int spawnY =
            GD.RandRange(0, 19);

        enemy.GlobalPosition =
            new Vector2(
                 16,
                 spawnY * 32 + 16
            );
        GD.Print($"Enemy spawned at X=0, ={spawnY}"
            );
    }

}