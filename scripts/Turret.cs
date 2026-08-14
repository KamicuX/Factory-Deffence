using Godot;

public partial class Turret : Node2D
{
    private const int BaseDamage = 25;
    private float damageMultiplier = 1.0f;
    private const float AttackRange = 180.0f;
    private const float AttackCooldown = 1.0f;

    private const int MaxHealth = 150;
    private int health;
    private Vector2I myCell;
    private Grid grid;

    private float attackTimer = 0.0f;

    public override void _Ready()
    {
        AddToGroup("turrets");
        ResearchManager researchManager =
        GetTree().Root.GetNode<ResearchManager>(
            "Game/ResearchManager"
            );

        SetDamageMultiplier(
            researchManager.GetTurretDamageMultiplier()
           );

        health = MaxHealth;

        grid = GetTree().Root.GetNode<Grid>("Game/Grid");
        if (grid != null)
        {
            myCell = grid.WorldToCell(GlobalPosition);
        }

        QueueRedraw();
    }
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    private int GetDamage()
    {
        return Mathf.RoundToInt(BaseDamage * damageMultiplier);
    }

    public override void _Process(double delta)
    {
        attackTimer -= (float)delta;

        if (attackTimer > 0)
            return;

        Enemy target = FindClosestEnemy();

        if (target != null)
        {
            Attack(target);
            attackTimer = AttackCooldown;
        }
    }

    private Enemy FindClosestEnemy()
    {
        Enemy closestEnemy = null;
        float closestDistance = AttackRange;
        Vector2 turretCenter =
        GlobalPosition + new Vector2(16, 16);

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
            {
               
                float distance =
                    turretCenter.DistanceTo(enemy.GlobalPosition);

                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }

    private void Attack(Enemy enemy)
    {
        int damage = GetDamage();

        enemy.TakeDamage(damage);
        // Notify enemy that it has been attacked by this turret so it can retaliate
        enemy.OnAttackedBy(this);

        if (DebugConfig.EnableLogs) GD.Print(
            $"Turret attacked enemy for {damage} damage"
        );
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (DebugConfig.EnableLogs) GD.Print($"Turret HP: {health}/{MaxHealth}");

        if (health <= 0)
        {
            if (DebugConfig.EnableLogs) GD.Print("Turret destroyed!");
            if (grid != null)
            {
                if (DebugConfig.EnableLogs) GD.Print($"Turret: calling RemoveObject for turret instance");
                grid.RemoveObject(this);
            }
            QueueFree();
        }
    }


    public override void _Draw()
    {
        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(32, 32)),
            Colors.DarkBlue,
            true
        );

        DrawRect(
            new Rect2(Vector2.Zero, new Vector2(32, 32)),
            Colors.Blue,
            false,
            3
        );

        DrawArc(
            new Vector2(16, 16),
            AttackRange,
            0,
            Mathf.Tau,
            64,
            Colors.Blue,
            1
        );
    }
}