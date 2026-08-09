using Godot;

public partial class Grid : Node2D
{
    private const int CellSize = 32;
    private const int GridWidth = 35;
    private const int GridHeight = 20;
    private string selectedBuilding = "";

    private Vector2I? previewCell = null;
    private bool buildingSelected = false;


    public override void _Ready()
    {
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.Pressed)
        {
            // Prawy przycisk myszy - anulowanie budowania
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                CancelBuilding();
                return;
            }

            // Lewy przycisk myszy - postawienie budynku
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                PlaceBuilding();
            }
        }
    }
    private void CancelBuilding()
    {
        buildingSelected = false;
        previewCell = null;

        QueueRedraw();

        GD.Print("Building cancelled");
    }
    private void PlaceBuilding()
    {
        if (!buildingSelected || !previewCell.HasValue)
            return;

        Vector2I cell = previewCell.Value;

        if (selectedBuilding == "Turret")
        {
            BuildTurret(cell);
        }

        GD.Print($"Building placed at X={cell.X}, Y={cell.Y}");

        buildingSelected = false;
        previewCell = null;

        QueueRedraw();
    }

    public override void _Draw()
    {
        // rysowanie siatki

        for (int x = 0; x <= GridWidth; x++)
        {
            float positionX = x * CellSize;

            DrawLine(
                new Vector2(positionX, 0),
                new Vector2(positionX, GridHeight * CellSize),
                Colors.Gray
            );
        }

        for (int y = 0; y <= GridHeight; y++)
        {
            float positionY = y * CellSize;

            DrawLine(
                new Vector2(0, positionY),
                new Vector2(GridWidth * CellSize, positionY),
                Colors.Gray
            );
        }


        // podgląd budynku

        if (buildingSelected && previewCell.HasValue)
        {
            Vector2I cell = previewCell.Value;

            Vector2 position = new Vector2(
                cell.X * CellSize,
                cell.Y * CellSize
            );

            DrawRect(
                new Rect2(
                    position,
                    new Vector2(CellSize, CellSize)
                ),
                Colors.Blue,
                true
            );
        }
    }


    public override void _Process(double delta)
    {
        if (!buildingSelected)
            return;

        Vector2 localPosition = ToLocal(GetGlobalMousePosition());

        int cellX = Mathf.FloorToInt(localPosition.X / CellSize);
        int cellY = Mathf.FloorToInt(localPosition.Y / CellSize);

        if (cellX >= 0 && cellX < GridWidth &&
            cellY >= 0 && cellY < GridHeight)
        {
            Vector2I newCell = new Vector2I(cellX, cellY);

            if (previewCell != newCell)
            {
                previewCell = newCell;
                QueueRedraw();
            }
        }
    }

    private void BuildTurret(Vector2I cell)
    {
        PackedScene turretScene =
            GD.Load<PackedScene>("res://scenes/Turret.tscn");

        Node2D turret = turretScene.Instantiate<Node2D>();

        GetParent().AddChild(turret);

        turret.Position = new Vector2(
            cell.X * CellSize,
            cell.Y * CellSize
        );

        buildingSelected = false;
        selectedBuilding = "";

        previewCell = null;

        QueueRedraw();

        GD.Print($"Turret built at X={cell.X}, Y={cell.Y}");
    }


    // WYBÓR BUDYNKU
    public void SelectBuilding(string buildingType)
    {
        buildingSelected = true;
        selectedBuilding = buildingType;

        GD.Print($"Building selected: {buildingType}");

        QueueRedraw();
    }
}