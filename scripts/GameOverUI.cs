using Godot;

public partial class GameOverUI : CanvasLayer
{
    private Label enemiesKilledLabel;
    private Label waveLabel;
    private Button restartButton;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        enemiesKilledLabel =
            GetNode<Label>(
                "CenterContainer/Panel/MarginContainer/VBoxContainer/EnemiesKilledLabel"
            );
        waveLabel =
             GetNode<Label>(
                "CenterContainer/Panel/MarginContainer/VBoxContainer/WaveLabel"
            );
        restartButton =
             GetNode<Button>(
                "CenterContainer/Panel/MarginContainer/VBoxContainer/RestartButton"
            );
        restartButton.Pressed += OnRestartPressed;

        Hide();
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    public void ShowGameOver()
    {
        GameManager gameManager =
            GetTree().Root.GetNode<GameManager>(
                "Game/GameManager"
            );
        WaveManager waveManager =
            GetTree().Root.GetNode<WaveManager>(
                "Game/WaveManager"
            );
      

        enemiesKilledLabel.Text =
            $"Enemies killed: {gameManager.EnemiesKilled}";
        waveLabel.Text =
            $"Wave reached: {waveManager.CurrentWave}";

        Show();
    }
}