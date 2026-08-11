using Godot;
using System.Collections.Generic;

public partial class Grid : Node2D
{
    private const int CellSize = 32;
    private const int GridWidth = 35;
    private const int GridHeight = 20;
    private string selectedBuilding = "";
    private Vector2I selectedBuildingSize = new Vector2I(1, 1);

    private Vector2I? previewCell = null;
    private bool buildingSelected = false;
    private bool placingHQ = true;
    private float enemySpawnTimer = -1.0f;
    private HQ playerHQ;
    private HashSet<Vector2I> occupiedCells = new HashSet<Vector2I>();


    public override void _Ready()
    {
        if (placingHQ)
        {
            buildingSelected = true;
            selectedBuilding = "HQ";
            selectedBuildingSize = new Vector2I(3, 3);

            GD.Print("Choose location for HQ");
            QueueRedraw();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.Pressed)
        {
            // Prawy przycisk myszy - anulowanie budowania
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (placingHQ)
                    return;

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

        if (!IsAreaAvailable(cell))
        {
            GD.Print("Cannot build here - area is occupied or outside the Grid.");
            return;
        }

        if (selectedBuilding == "Turret")
        {
            BuildTurret(cell);
        }
        else if (selectedBuilding == "Wall")
        {
            BuildWall(cell);
        }
        else if (selectedBuilding == "HQ")
        {
            BuildHQ(cell);
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
            Vector2 position = GetCellPosition(previewCell.Value);

            Vector2 size = new Vector2(
                selectedBuildingSize.X * CellSize,
                selectedBuildingSize.Y * CellSize
            );

            DrawRect(
                new Rect2(position, size),
                Colors.Blue,
                true
            );
        }
    }



    public override void _Process(double delta)
    {
        // TIMER PRZECIWNIKA
        if (enemySpawnTimer > 0)
        {
            enemySpawnTimer -= (float)delta;

            if (enemySpawnTimer <= 0)
            {
                SpawnEnemy();
            }
        }

        // PREVIEW BUDYNKU
        if (!buildingSelected)
            return;

        Vector2 localPosition =
            ToLocal(GetGlobalMousePosition());

        int cellX =
            Mathf.FloorToInt(localPosition.X / CellSize);

        int cellY =
            Mathf.FloorToInt(localPosition.Y / CellSize);

        if (cellX >= 0 && cellX < GridWidth &&
            cellY >= 0 && cellY < GridHeight)
        {
            Vector2I newCell =
                new Vector2I(cellX, cellY);

            if (previewCell != newCell)
            {
                previewCell = newCell;
                QueueRedraw();
            }
        }
    }
    private void SpawnEnemy()
    {
        PackedScene enemyScene =
        GD.Load<PackedScene>("res://scenes/Enemy.tscn");

        Enemy enemy =
            enemyScene.Instantiate<Enemy>();

        GetParent().AddChild(enemy);

        // Losowy rząd przy lewej krawędzi Gridu
        int spawnY = GD.RandRange(0, GridHeight - 1);

        enemy.Position = new Vector2(
            0,
            spawnY * CellSize
        );

        enemy.SetTargetHQ(playerHQ);

        GD.Print($"Enemy spawned at X=0, Y={spawnY}");
    }

    private Vector2 GetCellPosition(Vector2I cell)
    {
        return new Vector2(
            cell.X * CellSize,
            cell.Y * CellSize
        );
    }
    private bool IsAreaAvailable(Vector2I startCell)
    {
        for (int x = 0; x < selectedBuildingSize.X; x++)
        {
            for (int y = 0; y < selectedBuildingSize.Y; y++)
            {
                Vector2I cell = new Vector2I(
                    startCell.X + x,
                    startCell.Y + y
                );

                // Czy budynek wychodzi poza Grid?
                if (cell.X < 0 || cell.X >= GridWidth ||
                    cell.Y < 0 || cell.Y >= GridHeight)
                {
                    return false;
                }

                // Czy pole jest już zajęte?
                if (occupiedCells.Contains(cell))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void BuildHQ(Vector2I cell)
    {
        PackedScene hqScene =
            GD.Load<PackedScene>("res://scenes/HQ.tscn");

        Node2D hq = hqScene.Instantiate<Node2D>();

        GetParent().AddChild(hq);

        hq.Position = GetCellPosition(cell);

        playerHQ = hq as HQ;
        enemySpawnTimer = 10.0f;

        GD.Print("HQ placed! Enemy will spawn in 10 seconds.");

        // HQ zajmuje 3x3 pola
        for (int x = 0; x < selectedBuildingSize.X; x++)
        {
            for (int y = 0; y < selectedBuildingSize.Y; y++)
            {
                Vector2I occupiedCell = new Vector2I(cell.X + x, cell.Y + y);
                occupiedCells.Add(occupiedCell);
            }
        }

        placingHQ = false;
        buildingSelected = false;
        selectedBuilding = "";
        previewCell = null;

        QueueRedraw();

        GD.Print($"HQ built at X={cell.X}, Y={cell.Y}");
    }

    private void BuildTurret(Vector2I cell)
    {
        PackedScene turretScene =
            GD.Load<PackedScene>("res://scenes/Turret.tscn");

        Node2D turret = turretScene.Instantiate<Node2D>();

        GetParent().AddChild(turret);

        turret.Position = GetCellPosition(cell);

        buildingSelected = false;
        selectedBuilding = "";

        previewCell = null;
        occupiedCells.Add(cell);
        QueueRedraw();
        
        GD.Print($"Turret built at X={cell.X}, Y={cell.Y}");
    }


    private void BuildWall(Vector2I cell)
    {
        PackedScene wallScene =
            GD.Load<PackedScene>("res://scenes/Wall.tscn");

        Node2D wall = wallScene.Instantiate<Node2D>();

        GetParent().AddChild(wall);

        wall.Position = GetCellPosition(cell);

        buildingSelected = false;
        selectedBuilding = "";

        previewCell = null;

        QueueRedraw();

        GD.Print($"Wall built at X={cell.X}, Y={cell.Y}");
    }


    // WYBÓR BUDYNKU
    public void SelectBuilding(string buildingType)
    {

        if (placingHQ && buildingType != "HQ")
        {
            GD.Print("You must place the HQ first!");
            return;
        }

        buildingSelected = true;
        selectedBuilding = buildingType;

        if (buildingType == "HQ")
        {
            selectedBuildingSize = new Vector2I(3, 3);
        }
        else
        {
            selectedBuildingSize = new Vector2I(1, 1);
        }

        GD.Print($"Building selected: {buildingType}");
        GD.Print($"Building size: {selectedBuildingSize}");

        QueueRedraw();
    }
}