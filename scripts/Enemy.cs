using Godot;
using System.Collections.Generic;

public partial class Enemy : Node2D
{
    private const int Damage = 10;
    private const float AttackRange = 80.0f;
    private const float AttackCooldown = 1.0f;
    private int health = 100;

    private HQ targetHQ;
    private float attackTimer = 0.0f;
    private Grid grid;
    private List<Vector2> path = new List<Vector2>();
    private int pathIndex = 0;
    private float speed = 50.0f;
    private int knownGridVersion = -1;

    private Node2D currentTargetNode = null;

    // fallback periodic recalculation in case event missed
    private float pathRecalcTimer = 0.0f;
    private const float PathRecalcInterval = 0.5f;

    public override void _Ready()
    {
        AddToGroup("enemies");

        grid =
          GetTree().Root.GetNode<Grid>(
          "Game/Grid"
         );

        if (grid != null)
        {
            grid.OnGridChanged += OnGridChanged;
            knownGridVersion = grid.GridVersion;
            if (DebugConfig.EnableLogs) GD.Print($"Enemy ready. GridVersion={knownGridVersion}, grid!=null: {grid != null}");
        }

        Node hqNode =
        GetTree().GetFirstNodeInGroup("player_hq");

        if (hqNode is HQ hq)
        {
            targetHQ = hq;
        }
    }

    public void SetTargetHQ(HQ hq)
    {
        targetHQ = hq;
    }

    public override void _Process(double delta)
    {
        if (targetHQ == null)
            return;

        // If our current target was destroyed or freed, clear it so we can recalc path
        if (currentTargetNode != null && !IsInstanceValid(currentTargetNode))
        {
            ClearCurrentTarget();
        }

        // If currentTargetNode no longer exists in the grid (removed from occupiedMap), clear it
        if (currentTargetNode != null && grid != null)
        {
            Vector2I cell = grid.WorldToCell(currentTargetNode.GlobalPosition);
            var objAt = grid.GetObjectAtCell(cell);
            if (objAt == null || objAt != currentTargetNode)
            {
                ClearCurrentTarget();
            }
        }

        float dt = (float)delta;
        Vector2 hqCenter = targetHQ.GetCenter();

        Vector2 currentGoal = hqCenter;

        if (currentTargetNode != null)
        {
            currentGoal = currentTargetNode.GlobalPosition;
        }
        else if (grid != null)
        {
            Vector2I enemyCell =
                grid.WorldToCell(GlobalPosition);

            Vector2I? approachCell =
                grid.FindHQApproachCell(enemyCell);

            if (approachCell.HasValue)
            {
                currentGoal =
                    grid.GetCellCenter(
                        approachCell.Value
                    );
            }
            if (DebugConfig.EnableLogs) GD.Print(
    $"Enemy approach cell: {approachCell}"
);
        }

        float distance =
            GlobalPosition.DistanceTo(currentGoal);

        // -------------------------------------------------
        // ATTACK RANGE
        // -------------------------------------------------

        if (distance <= AttackRange)
        {
            attackTimer -= dt;

            if (attackTimer <= 0.0f)
            {
                Attack();
                attackTimer = AttackCooldown;
            }

            return;
        }

        // -------------------------------------------------
        // PATH RECALCULATION
        // -------------------------------------------------

        bool needRecalc = false;

        if (grid != null &&
            grid.GridVersion != knownGridVersion)
        {
            needRecalc = true;
        }

        pathRecalcTimer -= dt;

        if (path.Count == 0 ||
            pathIndex >= path.Count ||
            pathRecalcTimer <= 0.0f)
        {
            needRecalc = true;
        }

        if (needRecalc && grid != null)
        {
            // Recalculate toward current objective.
            currentGoal =
                currentTargetNode != null
                    ? currentTargetNode.GlobalPosition
                    : hqCenter;

            // Check for a blocking occupied cell along straight line to the goal. If present, prefer to attack it.
            Vector2I startCellCheck = grid.WorldToCell(GlobalPosition);
            Vector2I goalCellCheck = grid.WorldToCell(currentGoal);

            if (grid.TryGetBlockingCellAlongLine(startCellCheck, goalCellCheck, out Vector2I blockingCell))
            {
                var blockingObj = grid.GetObjectAtCell(blockingCell);
                if (blockingObj != null)
                {
                    // Prefer to attack walls (or any blocking object) on the direct line to the HQ
                    currentTargetNode = blockingObj;

                    Vector2I originCell = startCellCheck;
                    Vector2I? attackCell = grid.FindClosestWalkableAdjacent(blockingCell, originCell);
                    if (attackCell.HasValue)
                    {
                        Vector2 attackPos = grid.GetCellCenter(attackCell.Value);
                        path = grid.FindPath(GlobalPosition, attackPos);
                        pathIndex = 0;
                    }
                    else
                    {
                        // Try reachable attack positions near occupied cells
                        if (grid.TryFindReachableAttackPosition(originCell, goalCellCheck, out Vector2 attackPos2, out Vector2I occCell))
                        {
                            path = grid.FindPath(GlobalPosition, attackPos2);
                            pathIndex = 0;
                            currentTargetNode = grid.GetObjectAtCell(occCell);
                        }
                        else
                        {
                            // fallback: compute normal path to goal (will likely go around)
                            path = grid.FindPath(GlobalPosition, currentGoal);
                        }
                    }
                }
                else
                {
                    path = grid.FindPath(GlobalPosition, currentGoal);
                }
            }
            else
            {
                // No direct blocking object — compute normal path
                path = grid.FindPath(GlobalPosition, currentGoal);
            }

            var startCell = grid.WorldToCell(GlobalPosition);
            var goalCell = grid.WorldToCell(currentGoal);
            if (DebugConfig.EnableLogs) GD.Print($"Enemy recalced path. start={startCell}, goal={goalCell}, pathCount={(path==null?0:path.Count)}");
            if (DebugConfig.EnableLogs) GD.Print($"Enemy path: {(path==null?0:path.Count)} | Start: {startCell} | Goal: {goalCell}");

                // If initial A* failed (no path) try to find reachable attack position near occupied cells
                if ((path == null || path.Count == 0) && grid != null)
                {
                    Vector2I originCell = grid.WorldToCell(GlobalPosition);
                    if (grid.TryFindReachableAttackPosition(originCell, grid.WorldToCell(currentGoal), out Vector2 attackPos3, out Vector2I occ3))
                    {
                        path = grid.FindPath(GlobalPosition, attackPos3);
                        pathIndex = 0;
                        currentTargetNode = grid.GetObjectAtCell(occ3);
                        if (DebugConfig.EnableLogs) GD.Print($"Fallback attack pos found: {attackPos3} for occupied {occ3}");
                    }
                }

            pathIndex = 0;

            knownGridVersion =
                grid.GridVersion;

            pathRecalcTimer =
                PathRecalcInterval;

            // -------------------------------------------------
            // NO PATH TO HQ
            // -------------------------------------------------

            if (path.Count == 0 &&
                currentTargetNode == null)
            {
                Node2D blockingObject =
                 grid.FindBlockingObject(
                  GlobalPosition,
                    hqCenter
                 );

                if (blockingObject is Wall)
                {
                    currentTargetNode =
                        blockingObject;

                    Vector2I objectCell =
                        grid.WorldToCell(
                            blockingObject.GlobalPosition
                        );

                    Vector2I originCell =
                        grid.WorldToCell(
                            GlobalPosition
                        );

                    Vector2I? attackCell =
                        grid.FindClosestWalkableAdjacent(
                            objectCell,
                            originCell
                        );

                    if (attackCell.HasValue)
                    {
                        Vector2 attackPosition =
                            grid.GetCellCenter(
                                attackCell.Value
                            );

                        path =
                            grid.FindPath(
                                GlobalPosition,
                                attackPosition
                            );

                        pathIndex = 0;


                    }
                }
            }
        }

        // -------------------------------------------------
        // MOVEMENT
        // -------------------------------------------------

        if (path != null &&
            path.Count > 0 &&
            pathIndex < path.Count)
        {
            Vector2 waypoint =
                path[pathIndex];

            Vector2 direction =
                GlobalPosition.DirectionTo(
                    waypoint
                );

            GlobalPosition +=
                direction * speed * dt;

            if (GlobalPosition.DistanceTo(waypoint) < 4.0f)
            {
                pathIndex++;
            }
    else
    {
        if (DebugConfig.EnableLogs) GD.Print($"Enemy moving towards waypoint {pathIndex} at {waypoint}");
    }
        }
    }

    private void Attack()
    {
        // Target was destroyed or removed.
        if (currentTargetNode == null ||
            !IsInstanceValid(currentTargetNode))
        {
            currentTargetNode = null;
            path.Clear();
            pathIndex = 0;
            knownGridVersion = -1;

            return;
        }

        if (currentTargetNode is Wall wall)
        {
            wall.TakeDamage(Damage);

            if (!IsInstanceValid(wall))
            {
            if (DebugConfig.EnableLogs) GD.Print("Wall destroyed. Returning to HQ.");

                ClearCurrentTarget();
            }
            return;
        }

        if (currentTargetNode is HQ hq)
        {
            hq.TakeDamage(Damage);

            

            return;
        }

        if (currentTargetNode is Turret turret)
        {
            turret.TakeDamage(Damage);

            

            if (!IsInstanceValid(currentTargetNode))
            {
                currentTargetNode = null;
                path.Clear();
                pathIndex = 0;
                knownGridVersion = -1;
            }

            return;
        }

        // Unknown target type.
        currentTargetNode = null;
        path.Clear();
        pathIndex = 0;
        knownGridVersion = -1;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        

        if (health <= 0)
        {
            

            GameManager gameManager =
                GetTree().Root.GetNode<GameManager>(
                    "Game/GameManager"
                );

            gameManager.RegisterEnemyKill();

            QueueFree();
        }
    }
    private void ClearCurrentTarget()
    {
        currentTargetNode = null;
        path.Clear();
        pathIndex = 0;
        knownGridVersion = -1;
        pathRecalcTimer = 0.0f;
        if (DebugConfig.EnableLogs) GD.Print($"Enemy: ClearCurrentTarget called");
    }

    // Called by attackers (e.g., Turret) to mark them as a retaliatory target
    public void OnAttackedBy(Node2D attacker)
    {
        currentTargetNode = attacker;
        // force immediate path recalculation
        knownGridVersion = -1;
        path.Clear();
        pathIndex = 0;
    }

    private void OnGridChanged(int newVersion)
    {
        // When grid changes, force path recalculation
        knownGridVersion = -1;
        path.Clear();
        pathIndex = 0;
    }
   

    public override void _Draw()
    {
        DrawCircle(
           Vector2.Zero,
           5,
           Colors.Purple
       );
    }
}