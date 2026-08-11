using Godot;

public partial class HQ : Node2D
{
    private const int CellSize = 32;
    private const int SizeInCells = 3;
    public int Health = 1000;
    public int PlayerId = 1;

    public override void _Ready()
    {
        float gridWidth = 35 * CellSize;
        float gridHeight = 20 * CellSize;

        float hqSize = SizeInCells * CellSize;

        Position = new Vector2(
            (gridWidth - hqSize) / 2,
            (gridHeight - hqSize) / 2
        );

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
    public void TakeDamage(int damage)
    {
        Health -= damage;

        GD.Print($"HQ HP: {Health}");

        if (Health <= 0)
        {
            Health = 0;

            GD.Print("HQ DESTROYED!");
            GD.Print($"Player {PlayerId} LOSES!");
        }
    }
}