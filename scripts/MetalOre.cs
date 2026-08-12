using Godot;

public partial class MetalOre : Node2D
{
    private const int CellSize = 32;

    public override void _Ready()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawCircle(
            new Vector2(16, 16),
            10,
            Colors.DarkGray
        );

        DrawCircle(
            new Vector2(12, 13),
            3,
            Colors.LightGray
        );

        DrawCircle(
            new Vector2(20, 19),
            2,
            Colors.LightGray
        );
    }
}
