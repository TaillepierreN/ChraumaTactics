using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public UnitType unitType;
    [Header("Unit Base Stats")]
    public int baseHealth;
    public int baseAtk;
    public float baseMoveSpeed;
    public float baseAtkSpeed;
    public float baseRange;
    [Header("Unit Boosted Stats")]
    protected int boostedHealth;
    protected int boostedAtk;
    protected float boostedMoveSpeed;
    protected float boostedAtkSpeed;
    protected float boostedRange;

    [Header("Unit Current Stats")]
    protected int currentHealth;
    protected int currentAtk;
    protected float currentMoveSpeed;
    protected float currentAtkSpeed;
    protected float currentRange;

    [Header("Unit Spawn Position")]
    public Vector3 spawnPosition;

    /// <summary>
    /// Gets the current health of the unit.
    public int CurrentHealth => currentHealth;

    public virtual void Initialize()
    {
        SetCurrentStats(baseHealth, baseAtk, baseMoveSpeed, baseAtkSpeed, baseRange);
    }

    /// <summary>
    /// Sets the current stats of the unit.
    /// </summary>
    /// <param name="health"></param>
    /// <param name="atk"></param>
    /// <param name="moveSpeed"></param>
    /// <param name="atkSpeed"></param>
    /// <param name="range"></param>
    public virtual void SetCurrentStats(int health, int atk, float moveSpeed, float atkSpeed, float range)
    {
        currentHealth = health;
        currentAtk = atk;
        currentMoveSpeed = moveSpeed;
        currentAtkSpeed = atkSpeed;
        currentRange = range;
    }
    /// <summary>
    /// Applies damage to the unit and checks if it should be dead.
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // Explosion animation
            this.gameObject.SetActive(false);
        }
    }

    // Example of boost to stats at start of rounds?
    /*public virtual void UpdateBoostedStats(BonusManager manager)
    {
        boostedHealth = baseHealth + manager.GetHealthBonus();
        boostedAtk = baseAtk + manager.GetAttackBonus();
        boostedMoveSpeed = baseMoveSpeed + manager.GetMoveSpeedBonus();
        boostedAtkSpeed = baseAtkSpeed + manager.GetAttackSpeedBonus();
        boostedRange = baseRange + manager.GetRangeBonus();
        SetCurrentStats(boostedHealth, boostedAtk, boostedMoveSpeed, boostedAtkSpeed, boostedRange);
    }*/

    /// <summary>
    /// Resets the unit's stats to the boosted values and position .Call at game round end
    public virtual void ResetStats()
    {
        SetCurrentStats(boostedHealth, boostedAtk, boostedMoveSpeed, boostedAtkSpeed, boostedRange);
        transform.position = spawnPosition;
    }
}
