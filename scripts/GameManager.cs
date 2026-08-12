using Godot;

public partial class GameManager : Node
{
    public bool GameOver { get; private set; } = false;

    private int enemiesKilled = 0;

    public int EnemiesKilled => enemiesKilled;

    public void RegisterEnemyKill()
    {
        enemiesKilled++;
        GD.Print($"Enemies killed: {enemiesKilled}");
    }

    public void PlayerLost()
    {
        if (GameOver)
            return;

        GameOver = true;

        GD.Print("========== GAME OVER ==========");
        GD.Print("Player 1 LOSES!");

        GameOverUI gameOverUI =
            GetTree().Root.GetNode<GameOverUI>(
                "Game/GameOverUI"
            );

        gameOverUI.ShowGameOver();

        GetTree().Paused = true;
    }
}