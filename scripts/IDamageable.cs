public interface IDamageable
{
    // Apply direct instant damage. Implementing objects manage their own HP and death.
    void TakeDamage(int damage);
}
