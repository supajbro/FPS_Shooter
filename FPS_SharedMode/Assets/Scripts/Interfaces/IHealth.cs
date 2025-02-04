public interface IHealth
{
    float Health { get; }
    float MaxHealth { get; }

    void TakeDamage(float amount);
    void Heal(float amount);
}
