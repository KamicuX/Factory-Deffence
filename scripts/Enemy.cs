using Godot;

public partial class Enemy : Node2D
{
    private const int Damage = 10;
    private const float AttackRange = 40.0f;
    private const float AttackCooldown = 1.0f;
    private int health = 100;

    private HQ targetHQ;
    private float attackTimer = 0.0f;

    public override void _Ready()
    {
        AddToGroup("enemies");
        targetHQ = GetNode<HQ>("../HQ");
    }

    public override void _Process(double delta)
    {
        if (targetHQ == null)
            return;

        float distance = GlobalPosition.DistanceTo(targetHQ.GlobalPosition);

        if (distance > AttackRange)
        {
            Vector2 direction = GlobalPosition.DirectionTo(targetHQ.GlobalPosition);

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

            QueueFree();
        }
    }
   

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 12, Colors.Purple);
    }
}