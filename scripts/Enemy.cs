using Godot;

public partial class Enemy : Node2D
{
    private const int Damage = 10;
    private const float AttackRange = 80.0f;
    private const float AttackCooldown = 1.0f;
    private int health = 100;

    private HQ targetHQ;
    private float attackTimer = 0.0f;

    public override void _Ready()
    {
        AddToGroup("enemies");
        
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
        Vector2 hqCenter = targetHQ.GetCenter();

        float distance =
        GlobalPosition.DistanceTo(hqCenter);

        if (distance > AttackRange)
        {

            Vector2 direction =
                GlobalPosition.DirectionTo(hqCenter);

            GlobalPosition += direction * 50.0f * (float)delta;
        }
        else
        {
            attackTimer -= (float)delta;

            if (attackTimer <= 0)
            {
                Attack();
                attackTimer = AttackCooldown;
            }
        }
    }

    private void Attack()
    {
        targetHQ.TakeDamage(Damage);

        GD.Print($"Enemy attacked HQ for {Damage} damage");
    }
    public void TakeDamage(int damage)
    {
        health -= damage;

        GD.Print($"Enemy HP: {health}");

        if (health <= 0)
        {
            GD.Print("Enemy destroyed!");

            GameManager gameManager =
                GetTree().Root.GetNode<GameManager>(
                    "Game/GameManager"
                );

            gameManager.RegisterEnemyKill();

            QueueFree();
        }
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