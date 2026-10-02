using Godot;

public partial class DamageManager : Node
{
    public static DamageManager Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    // Apply damage to a target implementing IDamageable.
    // Optional source can be provided by attackers for notification (retaliation, logs, etc.).
    public void ApplyDamage(IDamageable target, int amount, Node2D source = null)
    {
        if (target == null)
            return;

        // Delegate the actual HP/state change to the target
        target.TakeDamage(amount);

        // If the target is an Enemy and a source was provided, notify it for potential retaliation
        if (source != null && target is Enemy enemy)
        {
            enemy.OnAttackedBy(source);
        }
    }
}
