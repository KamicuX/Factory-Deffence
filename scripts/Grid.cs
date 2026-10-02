using Godot;
using System.Collections.Generic;

public partial class Grid : Node2D
{
    private const int CellSize = 32;
    private const int GridWidth = 35;
    private const int GridHeight = 20;
    // Koszty budynków (metal)
    private const int CostMiner = 20;
    private const int CostTurret = 50;
    private const int CostWall = 10;
    private const int CostHQ = 0; // HQ is free
    private string selectedBuilding = "";
    private Vector2I selectedBuildingSize = new Vector2I(1, 1);

    private Vector2I? previewCell = null;
    private bool buildingSelected = false;
    private bool placingHQ = true;

    private HashSet<Vector2I> metalOreCells =
    new HashSet<Vector2I>();
    private const int MetalOreAmount = 15;

    private HQ playerHQ;

    private AStarGrid2D pathfindingGrid;
    private WaveManager waveManager;
    private HashSet<Vector2I> occupiedCells = new HashSet<Vector2I>();
    // Map occupied cells to the node that occupies them (for targeting)
    private Dictionary<Vector2I, Node2D> occupiedMap = new Dictionary<Vector2I, Node2D>();

    // Grid versioning and path cache for efficient repeated queries
    private int gridVersion = 0;
    private Dictionary<(Vector2I, Vector2I), (int version, List<Vector2> path)> pathCache =
        new Dictionary<(Vector2I, Vector2I), (int, List<Vector2>)>();

    public event System.Action<int> OnGridChanged;
    public int GridVersion => gridVersion;

    


    public override void _Ready()
    {

        InitializePathfinding();

        waveManager =
            GetParent().GetNode<WaveManager>("WaveManager");

        GenerateMetalOre();


        if (placingHQ)
        {
            buildingSelected = true;
            selectedBuilding = "HQ";
            selectedBuildingSize = new Vector2I(3, 3);

            if (DebugConfig.EnableLogs) GD.Print("Choose location for HQ");
            QueueRedraw();
        }
    }

    // Try to find a reachable attack position adjacent to any occupied cell near goal.
    public bool TryFindReachableAttackPosition(Vector2I originCell, Vector2I goalCell, out Vector2 attackPos, out Vector2I occupiedCell)
    {
        attackPos = Vector2.Zero;
        occupiedCell = new Vector2I();

        // Order occupied cells by distance to goalCell (closest first)
        var candidates = new List<Vector2I>(occupiedMap.Keys);
        candidates.Sort((a, b) =>
        {
            int da = Mathf.Abs(a.X - goalCell.X) + Mathf.Abs(a.Y - goalCell.Y);
            int db = Mathf.Abs(b.X - goalCell.X) + Mathf.Abs(b.Y - goalCell.Y);
            return da.CompareTo(db);
        });

        Vector2 originWorld = GetCellCenter(originCell);

        foreach (var occ in candidates)
        {
            var adj = FindClosestWalkableAdjacent(occ, originCell);
            if (!adj.HasValue)
                continue;

            Vector2 candidateWorld = GetCellCenter(adj.Value);
            var path = FindPath(originWorld, candidateWorld);
            if (path != null && path.Count > 0)
            {
                attackPos = candidateWorld;
                occupiedCell = occ;
                return true;
            }
        }

        return false;
    }

    // Scan cells along line from start to goal (grid cells) and return first occupied cell encountered (closest to start)
    public bool TryGetBlockingCellAlongLine(Vector2I start, Vector2I goal, out Vector2I blockingCell)
    {
        blockingCell = new Vector2I();

        int x0 = start.X;
        int y0 = start.Y;
        int x1 = goal.X;
        int y1 = goal.Y;

        int dx = System.Math.Abs(x1 - x0);
        int dy = System.Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            var cell = new Vector2I(x, y);
            // skip the starting cell (enemy cell) so we don't target self
            if (!(cell == start))
            {
                if (occupiedMap.ContainsKey(cell))
                {
                    blockingCell = cell;
                    return true;
                }
            }

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return false;
    }


    private void InitializePathfinding()
    {
        pathfindingGrid = new AStarGrid2D();

        pathfindingGrid.Region =
            new Rect2I(
                0,
                0,
                GridWidth,
                GridHeight
            );

        pathfindingGrid.CellSize =
            new Vector2(CellSize, CellSize);

        pathfindingGrid.DiagonalMode =
            AStarGrid2D.DiagonalModeEnum.Never;

        pathfindingGrid.Update();
        if (DebugConfig.EnableLogs) GD.Print(
    $"AStar initialized: Region={pathfindingGrid.Region}, CellSize={pathfindingGrid.CellSize}"
);

        if (DebugConfig.EnableLogs) GD.Print(
            $"AStar test path count: " +
            pathfindingGrid.GetIdPath(
                new Vector2I(0, 0),
                new Vector2I(10, 10)
            ).Count

);
    }
    private void UpdatePathfindingCell(Vector2I cell)
    {
        if (pathfindingGrid == null)
            return;

        if (!IsInsideGrid(cell))
            return;

        bool blocked = occupiedCells.Contains(cell);

        pathfindingGrid.SetPointSolid(
            cell,
            blocked
        );
        // Ensure A* internal connectivity is updated after changing solidity
        pathfindingGrid.Update();

        if (DebugConfig.EnableLogs) GD.Print(
    $"A* cells: " +
    $"(1,1)={pathfindingGrid.IsPointSolid(new Vector2I(1, 1))} | " +
    $"(5,5)={pathfindingGrid.IsPointSolid(new Vector2I(5, 5))} | " +
    $"(10,10)={pathfindingGrid.IsPointSolid(new Vector2I(10, 10))} | " +
    $"(20,10)={pathfindingGrid.IsPointSolid(new Vector2I(20, 10))}"
);

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
    private void GenerateMetalOre()
    {
        PackedScene oreScene =
            GD.Load<PackedScene>(
                "res://scenes/MetalOre.tscn"
            );

        int generated = 0;

        while (generated < MetalOreAmount)
        {
            int x = GD.RandRange(0, GridWidth - 1);
            int y = GD.RandRange(0, GridHeight - 1);

            Vector2I cell = new Vector2I(x, y);

            // Nie generuj dwóch złóż na tym samym polu
            if (metalOreCells.Contains(cell))
                continue;

            // Nie generuj na zajętym polu
            if (occupiedCells.Contains(cell))
                continue;

            MetalOre ore =
                oreScene.Instantiate<MetalOre>();

            ore.Position =
                GetCellPosition(cell);

            GetParent().CallDeferred(
               Node.MethodName.AddChild,
                ore
              );

            metalOreCells.Add(cell);

            generated++;

        if (DebugConfig.EnableLogs) GD.Print(
            $"Metal Ore generated at X={x}, Y={y}"
        );
        }
    }
    private void CancelBuilding()
    {
        buildingSelected = false;
        previewCell = null;

        QueueRedraw();

        if (DebugConfig.EnableLogs) GD.Print("Building cancelled");
    }
    private void PlaceBuilding()
    {
        if (!buildingSelected || !previewCell.HasValue)
            return;

        Vector2I cell = previewCell.Value;

        if (selectedBuilding == "Miner" &&
            !metalOreCells.Contains(cell))
        {
            if (DebugConfig.EnableLogs) GD.Print("Miner can only be built on Metal Ore!");
            return;
        }

        if (!IsAreaAvailable(cell))
        {
            if (DebugConfig.EnableLogs) GD.Print("Cannot build here - area is occupied or outside the Grid.");
            return;
        }

        // Sprawdź czy ResourceManager jest dostępny
        if (ResourceManager.Instance == null)
        {
            if (DebugConfig.EnableLogs) GD.Print("No ResourceManager instance available - cannot charge for building.");
            return;
        }

        // Spróbuj pobrać koszt przed postawieniem budynku
        int cost = 0;
        if (selectedBuilding == "Turret") cost = CostTurret;
        else if (selectedBuilding == "Wall") cost = CostWall;
        else if (selectedBuilding == "HQ") cost = CostHQ;
        else if (selectedBuilding == "Miner") cost = CostMiner;

        if (cost > 0)
        {
            bool paid = ResourceManager.Instance.TrySpend(ResourceType.Metal, cost);
            if (!paid)
            {
                if (DebugConfig.EnableLogs) GD.Print($"Not enough Metal to build {selectedBuilding}. Required: {cost}");
                return;
            }
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
        else if (selectedBuilding == "Miner")
        {
            BuildMiner(cell);

        }

        if (DebugConfig.EnableLogs) GD.Print($"Building placed at X={cell.X}, Y={cell.Y}");

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
    

    private Vector2 GetCellPosition(Vector2I cell)
    {
        return new Vector2(
            cell.X * CellSize,
            cell.Y * CellSize
        );
    }
    public Vector2 GetCellCenter(Vector2I cell)
    {
        Vector2 pos = GetCellPosition(cell);
        return pos + new Vector2(CellSize / 2.0f, CellSize / 2.0f);
    }
    public Vector2I? FindHQApproachCell(Vector2I originCell)
    {
        if (playerHQ == null)
            return null;

        Vector2I hqCell = WorldToCell(playerHQ.GetCenter());

        Vector2I bestCell = new Vector2I();
        int bestDistance = int.MaxValue;
        bool found = false;

        // HQ zajmuje 3x3 pola.
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Vector2I hqPart =
                    new Vector2I(
                        hqCell.X + x,
                        hqCell.Y + y
                    );

                Vector2I[] directions =
                {
                new Vector2I(1, 0),
                new Vector2I(-1, 0),
                new Vector2I(0, 1),
                new Vector2I(0, -1)
            };

                foreach (Vector2I direction in directions)
                {
                    Vector2I candidate =
                        hqPart + direction;

                    if (!IsInsideGrid(candidate))
                        continue;

                    if (!IsCellWalkable(candidate))
                        continue;

                    int distance =
                        Mathf.Abs(candidate.X - originCell.X) +
                        Mathf.Abs(candidate.Y - originCell.Y);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCell = candidate;
                        found = true;
                    }
                }
            }
        }

        if (!found)
            return null;

        return bestCell;
    }

    public Vector2I WorldToCell(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.X / CellSize);
        int y = Mathf.FloorToInt(worldPos.Y / CellSize);
        return new Vector2I(x, y);
    }

    private bool IsCellWalkable(Vector2I cell)
    {
        if (!IsInsideGrid(cell))
            return false;

        return !occupiedCells.Contains(cell);
    }

    public bool IsInsideGrid(Vector2I cell)
    {
        return cell.X >= 0 &&
               cell.X < GridWidth &&
               cell.Y >= 0 &&
               cell.Y < GridHeight;
    }


    public Node2D GetObjectAtCell(Vector2I cell)
    {
        if (occupiedMap.TryGetValue(cell, out var node))
            return node;
        return null;
    }
    public bool IsCellOccupied(Vector2I cell)
    {
        return occupiedCells.Contains(cell);
    }
    public bool TryGetObjectAtCell(Vector2I cell, out Node2D node)
    {

        return occupiedMap.TryGetValue(cell, out node);
    }
    public Node2D FindBlockingObject(Vector2 startWorld, Vector2 targetWorld)
    {
        Vector2I start = WorldToCell(startWorld);
        Vector2I target = WorldToCell(targetWorld);

        if (!IsInsideGrid(start) || !IsInsideGrid(target))
            return null;

        Vector2I current = start;

        for (int i = 0; i < GridWidth * GridHeight; i++)
        {
            if (current == target)
                return null;

            Vector2I[] directions =
            {
            new Vector2I(1, 0),
            new Vector2I(-1, 0),
            new Vector2I(0, 1),
            new Vector2I(0, -1)
        };

            Vector2I bestCell = current;
            int bestDistance = int.MaxValue;

            foreach (Vector2I direction in directions)
            {
                Vector2I neighbor = current + direction;

                if (!IsInsideGrid(neighbor))
                    continue;

                if (occupiedMap.TryGetValue(
                    neighbor,
                    out Node2D blockingObject))
                {
                    if (blockingObject is Wall)
                        return blockingObject;
                }

                if (occupiedCells.Contains(neighbor))
                    continue;

                int distance =
                    Mathf.Abs(neighbor.X - target.X) +
                    Mathf.Abs(neighbor.Y - target.Y);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCell = neighbor;
                }
            }

            if (bestCell == current)
                return null;

            current = bestCell;
        }

        return null;
    }

    public Node2D GetObjectAtWorld(Vector2 worldPos)
    {
        return GetObjectAtCell(WorldToCell(worldPos));
    }

    // Remove occupied cell when object is destroyed
    public void RemoveOccupiedCell(Vector2I cell)
    {
        if (occupiedCells.Contains(cell))
        {
            occupiedCells.Remove(cell);
        }

        if (occupiedMap.ContainsKey(cell))
        {
            occupiedMap.Remove(cell);
        }

        UpdatePathfindingCell(cell);
        if (DebugConfig.EnableLogs) GD.Print($"Grid: RemoveOccupiedCell at {cell}");
        MarkGridChanged();
    }
    public void RemoveObject(Node2D objectNode)
    {
        if (objectNode == null)
            return;

        List<Vector2I> cellsToRemove = new List<Vector2I>();

        foreach (var entry in occupiedMap)
        {
            if (entry.Value == objectNode)
            {
                cellsToRemove.Add(entry.Key);
            }
        }

        if (cellsToRemove.Count == 0)
            return;

        foreach (Vector2I cell in cellsToRemove)
        {
            occupiedCells.Remove(cell);
            occupiedMap.Remove(cell);
            UpdatePathfindingCell(cell);
        }
        if (DebugConfig.EnableLogs) GD.Print($"Grid: RemoveObject removed {cellsToRemove.Count} cells for {objectNode}");
        MarkGridChanged();
    }

    // Find the closest walkable adjacent cell to targetCell, measured from originCell. Returns null if none.
    public Vector2I? FindClosestWalkableAdjacent(Vector2I targetCell, Vector2I originCell)
    {
        Vector2I[] dirs = new Vector2I[] { new Vector2I(1,0), new Vector2I(-1,0), new Vector2I(0,1), new Vector2I(0,-1) };
        Vector2I? best = null;
        int bestDist = int.MaxValue;

        foreach (var d in dirs)
        {
            Vector2I n = new Vector2I(targetCell.X + d.X, targetCell.Y + d.Y);
            if (n.X < 0 || n.X >= GridWidth || n.Y < 0 || n.Y >= GridHeight)
                continue;

            if (occupiedCells.Contains(n))
                continue;

            int dist = Mathf.Abs(n.X - originCell.X) + Mathf.Abs(n.Y - originCell.Y);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }

        return best;
    }

    private void MarkGridChanged()
    {
        gridVersion++;
        pathCache.Clear();
        OnGridChanged?.Invoke(gridVersion);
    }

    public List<Vector2> FindPath(
    Vector2 startWorld,
    Vector2 targetWorld)
    {
        Vector2I start = WorldToCell(startWorld);
        Vector2I goal = WorldToCell(targetWorld);
        var key = (start, goal);
        if (pathCache.TryGetValue(key, out var cached) && cached.version == gridVersion)
        {
            return new List<Vector2>(cached.path);
        }

        if (DebugConfig.EnableLogs) GD.Print(
            $"A* Request: {start} -> {goal} | " +
            $"Occupied cells: {occupiedCells.Count}"
        );

        if (!IsInsideGrid(start) ||
            !IsInsideGrid(goal))
        {
            if (DebugConfig.EnableLogs) GD.Print(
                $"A* FAILED: Start or Goal outside grid. " +
                $"Start={start}, Goal={goal}"
            );

            return new List<Vector2>();
        }

        // Enemy może znajdować się na swojej aktualnej komórce.
        pathfindingGrid.SetPointSolid(
            start,
            false
        );

        // Cel ścieżki również musi być dostępny dla A*.
        // Dotyczy to szczególnie komórki HQ.
        pathfindingGrid.SetPointSolid(
            goal,
            false
        );

        if (DebugConfig.EnableLogs) GD.Print(
            $"START solid = " +
            $"{pathfindingGrid.IsPointSolid(start)}"
        );

        if (DebugConfig.EnableLogs) GD.Print(
            $"GOAL solid = " +
            $"{pathfindingGrid.IsPointSolid(goal)}"
        );

        // ---------------------------------------------
        // A*
        // ---------------------------------------------

        Godot.Collections.Array<Vector2I> pathCells =
            pathfindingGrid.GetIdPath(
                start,
                goal
            );

        // ---------------------------------------------
        // BRAK ŚCIEŻKI
        // ---------------------------------------------

        if (pathCells == null ||
            pathCells.Count == 0)
        {
            if (DebugConfig.EnableLogs) GD.Print(
                $"A* FAILED: Start={start}, Goal={goal}"
            );

            return new List<Vector2>();
        }

        // ---------------------------------------------
        // ŚCIEŻKA ZNALEZIONA
        // ---------------------------------------------

        if (DebugConfig.EnableLogs) GD.Print(
            $"A* found path: " +
            $"{pathCells.Count} cells | " +
            $"Start: {start} -> Goal: {goal}"
        );

        List<Vector2> path =
            new List<Vector2>();

        foreach (Vector2I cell in pathCells)
        {
            path.Add(
                GetCellCenter(cell)
            );
        }

        return path;
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

                if (metalOreCells.Contains(cell) &&
                    selectedBuilding != "Miner")
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

        // HQ zajmuje 3x3 pola
        for (int x = 0; x < selectedBuildingSize.X; x++)
        {
            for (int y = 0; y < selectedBuildingSize.Y; y++)
            {
                Vector2I occupiedCell = new Vector2I(cell.X + x, cell.Y + y);
                occupiedCells.Add(occupiedCell);
                occupiedMap[occupiedCell] = hq;
                UpdatePathfindingCell(occupiedCell);
            }
        }

        placingHQ = false;
        buildingSelected = false;
        selectedBuilding = "";
        previewCell = null;

        QueueRedraw();

        // notify listeners that grid changed (HQ occupies multiple cells)
        MarkGridChanged();

        waveManager.StartWaveCountdown();
    }

    private void BuildMiner(Vector2I cell)
    {
        PackedScene minerScene =
            GD.Load<PackedScene>(
                "res://scenes/Miner.tscn"
            );

        Node2D miner =
            minerScene.Instantiate<Node2D>();

        GetParent().AddChild(miner);

        miner.Position =
            GetCellPosition(cell);

        occupiedCells.Add(cell);
        occupiedMap[cell] = miner;
        UpdatePathfindingCell(cell);
        MarkGridChanged();

        buildingSelected = false;
        selectedBuilding = "";
        previewCell = null;

        QueueRedraw();

        
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
        occupiedMap[cell] = turret;
        UpdatePathfindingCell(cell);
        MarkGridChanged();
        QueueRedraw();

        
    }


    private void BuildWall(Vector2I cell)
    {
        PackedScene wallScene =
            GD.Load<PackedScene>("res://scenes/Wall.tscn");

        Node2D wall = wallScene.Instantiate<Node2D>();

        GetParent().AddChild(wall);

        wall.Position = GetCellPosition(cell);

        occupiedCells.Add(cell);
        occupiedMap[cell] = wall;

        UpdatePathfindingCell(cell);
        MarkGridChanged();

        buildingSelected = false;
        selectedBuilding = "";

        previewCell = null;

        QueueRedraw();

        
    }


    // WYBÓR BUDYNKU
    public void SelectBuilding(string buildingType)
    {

        if (placingHQ && buildingType != "HQ")
        {
        
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

        QueueRedraw();
    }
}