using Godot;

namespace ShroomGameReal.scenes;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Export]
    public float maxHealth;
    
    [Export]
    public float currentHealth;
    
    public bool IsDead => currentHealth <= 0f;

    public void Damage(float amount)
    {
        if (IsDead)
            return;
        
        currentHealth -= amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (IsDead)
            EmitSignalOnDeath();
    }

    [Signal]
    public delegate void OnDeathEventHandler();
}