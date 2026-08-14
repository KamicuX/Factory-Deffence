using Godot;

public partial class Wall : Node2D
{
    private const int Size = 32;
    private const int MaxHealth = 200;

    private int health;
    private Vector2I myCell;
    private Grid grid;

    public override void _Ready()
    {
        health = MaxHealth;

        grid = GetTree().Root.GetNode<Grid>("Game/Grid");
        if (grid != null)
        {
            myCell = grid.WorldToCell(GlobalPosition);
        }

        QueueRedraw();

        if (DebugConfig.EnableLogs) GD.Print(
            $"Wall created with {health} HP"
        );
    }

    private void Destroy()
    {
        if (DebugConfig.EnableLogs) GD.Print("Wall destroyed!");
        if (grid != null)
        {
            grid.RemoveObject(this);
        }

        QueueFree();
    }

    public int GetHealth()
    {
        return health;
    }


    public int GetMaxHealth()
    {
        return MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (DebugConfig.EnableLogs) GD.Print(
            $"Wall HP: {health}/{MaxHealth}"
        );

        if (health <= 0)
        {
            Destroy();
        }
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