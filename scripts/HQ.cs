using Godot;

public partial class HQ : Node2D
{
    private const int CellSize = 32;
    private const int SizeInCells = 3;
    public int Health = 1000;
    public int PlayerId = 1;

    public override void _Ready()
    {
        AddToGroup("player_hq");
        QueueRedraw();
    }

    public override void _Draw()
    {
        Vector2 size = new Vector2(
            SizeInCells * CellSize,
            SizeInCells * CellSize
        );

        DrawRect(
            new Rect2(Vector2.Zero, size),
            Colors.DarkRed,
            true
        );

        DrawRect(
            new Rect2(Vector2.Zero, size),
            Colors.Red,
            false,
            3
        );
    }
    public Vector2 GetCenter()
    {
        return GlobalPosition + new Vector2(
            SizeInCells * CellSize / 2,
            SizeInCells * CellSize / 2
        );
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (DebugConfig.EnableLogs) GD.Print($"HQ HP: {Health}");

        if (Health <= 0)
        {
            Health = 0;

            if (DebugConfig.EnableLogs) GD.Print("HQ DESTROYED!");
            if (DebugConfig.EnableLogs) GD.Print($"Player {PlayerId} LOSES!");

            GameManager gameManager =
        GetTree().Root.GetNode<GameManager>("Game/GameManager");

            gameManager.PlayerLost();
        }
    }
}