using Godot;

public partial class Building : Node2D
{
    [Export]
    public Vector2I Size { get; set; } = new Vector2I(1, 1);

    public Vector2I GridPosition { get; private set; }

    public void SetGridPosition(Vector2I position)
    {
        GridPosition = position;
    }

    public override void _Draw()
    {
        Vector2 pixelSize = new Vector2(
            Size.X * 32,
            Size.Y * 32
            );

        DrawRect(
            new Rect2(Vector2.Zero, pixelSize),
            Colors.Blue,
            true
            );

    }
}



