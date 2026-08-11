using Godot;

public partial class Wall : Node2D
{
    private const int Size = 32;

    public override void _Ready()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(Size, Size)),
            Colors.Gray,
            true
        );

        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(Size, Size)),
            Colors.DarkGray,
            false,
            3
        );
    }
}