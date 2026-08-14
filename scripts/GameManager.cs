using Godot;

public partial class GameManager : Node
{
    public bool GameOver { get; private set; } = false;

    private int enemiesKilled = 0;

    public int EnemiesKilled => enemiesKilled;

    public void RegisterEnemyKill()
    {
        enemiesKilled++;
        if (DebugConfig.EnableLogs) GD.Print($"Enemies killed: {enemiesKilled}");
    }

    public void PlayerLost()
    {
        if (GameOver)
            return;

        GameOver = true;

        if (DebugConfig.EnableLogs) GD.Print("========== GAME OVER ==========");
        if (DebugConfig.EnableLogs) GD.Print("Player 1 LOSES!");

        GameOverUI gameOverUI =
            GetTree().Root.GetNode<GameOverUI>(
                "Game/GameOverUI"
            );

        gameOverUI.ShowGameOver();

        GetTree().Paused = true;
    }
}