using Godot;

public partial class Miner : Node2D
{
    [Export]
    public MinerData Data { get; set; }

    private int health;
    private float productionTimer;

    private const int WidthInCells = 1;
    private const int HeightInCells = 1;
    private const int CellSize = 32;

    private Grid grid;
    private Vector2I myCell;

    public override void _Ready()
    {
        if (Data == null)
        {
            GD.PushError("MinerData is not assigned to Miner.");
            SetProcess(false);
            return;
        }

        health = Data.MaxHealth;
        productionTimer = Data.ProductionInterval;

        grid = GetTree().Root.GetNode<Grid>("Game/Grid");

        if (grid != null)
        {
            myCell = grid.WorldToCell(GlobalPosition);
        }

        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (Data == null || ResourceManager.Instance == null)
            return;

        productionTimer -= (float)delta;

        if (productionTimer <= 0.0f)
        {
            ProduceResources();

            productionTimer = Data.ProductionInterval;
        }
    }

    private void ProduceResources()
    {
        ResourceManager.Instance.Add(
            ResourceType.Metal,
            Data.ProductionAmount
        );

        if (DebugConfig.EnableLogs)
        {
            GD.Print(
                $"Miner produced {Data.ProductionAmount} Metal. " +
                $"Total: {ResourceManager.Instance.GetAmount(ResourceType.Metal)}/" +
                $"{ResourceManager.Instance.GetLimit(ResourceType.Metal)}"
            );
        }
    }

    public void TakeDamage(int damage)
    {
        int previousHealth = health;

        health -= damage;

        if (DebugConfig.EnableLogs)
        {
            GD.Print(
                $"Miner took {damage} dmg: " +
                $"{previousHealth} -> {health}/{Data.MaxHealth}"
            );
        }

        if (health <= 0)
        {
            Destroy();
        }

        QueueRedraw();
    }

    private void Destroy()
    {
        if (DebugConfig.EnableLogs)
        {
            GD.Print("Miner destroyed!");
        }

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
        return Data != null ? Data.MaxHealth : 0;
    }

    public override void _Draw()
    {
        Vector2 size = new Vector2(
            WidthInCells * CellSize,
            HeightInCells * CellSize
        );

        DrawRect(
            new Rect2(Vector2.Zero, size),
            Colors.Gray
        );

        DrawRect(
            new Rect2(Vector2.Zero, size),
            Colors.Black,
            false,
            2.0f
        );
    }
}