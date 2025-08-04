using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit : MonoBehaviour
{
    #region Unit Properties
    public bool DebugMode = false;
    public UnitType unitType;
    public Team team;
    protected Squad squad;


    [Header("Unit Base Stats")]
    public int baseHealth;
    public int baseAtk;
    public float baseMoveSpeed;
    public float baseAtkSpeed;
    public float baseRange;


    [Header("Unit Boosted Stats")]
    // These stats will be modified by bonuses or boosts during the game.
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
    protected float currentAtkRange;


    [Header("Unit Spawn Position")]
    /// <summary>The position where the unit spawns at the start of the game or round.</summary>
    public Vector3 spawnPosition;


    [Header("Unit NavMesh Agent")]
    protected NavMeshAgent agent;


    [Header("Unit Animator")]
    protected Animator animator;


    [Header("Unit Detection settings")]
    [SerializeField] private float detectionRadius = 100f;
    private float detectionInterval = 0.2f;
    private float detectionTimer = 0f;
    private HashSet<Unit> knownFriendlies = new(); // if we never use it for anything else than detect enemies, we should be able to remove it without performance issues(maybe gain even)
    private LayerMask detectionMask = ~0; // all layers for now, TODO: set to only units layer
    private Unit currentTarget = null;



    [Header("State")]
    public bool RoundStarted = false;
    public bool IsMoving = false;
    public bool IsAttacking = false;

    #endregion

    #region Unit Events

    /// <summary>Event triggered when the unit dies.</summary>
    public event System.Action OnUnitDeath;

    #endregion

    #region Unit Getters
    /// <summary>
    /// Gets the current health of the unit.
    public int CurrentHealth => currentHealth;

    #endregion

    #region Unity callbacks
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        //spawnPosition = transform.position;
    }

    protected virtual void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (DebugMode)
            RoundStarted = true;
        if (!RoundStarted)
                return;
        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            DetectEnemies();
        }
    }
    #endregion

    #region Stats

    /// <summary>Initializes the unit's stats based on base values.</summary>
    public virtual void Initialize()
    {
        SetCurrentStats(baseHealth, baseAtk, baseMoveSpeed, baseAtkSpeed, baseRange);
    }

    /// <summary>Sets the team of the unit.</summary>
    public virtual void SetTeam(Team newTeam)
    {
        team = newTeam;
    }

    /// <summary>Sets the squad of the unit.</summary>
    /// <param name="newSquad"></param>
    public void SetSquad(Squad newSquad)
    {
        squad = newSquad;
    }

    /// <summary>Sets the current stats of the unit.</summary>
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
        currentAtkRange = range;
        agent.speed = currentMoveSpeed;
    }

    //TODO 
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

    /// <summary>Resets the unit's stats to the boosted values and position .Call at game round end</summary>
    public virtual void ResetStats()
    {
        SetCurrentStats(boostedHealth, boostedAtk, boostedMoveSpeed, boostedAtkSpeed, boostedRange);
        transform.position = spawnPosition;
        RoundStarted = false;
        IsAttacking = false;
        currentTarget = null;
        IsMoving = false;
    }
    #endregion

    #region Movement mecanics

    /// <summary>Moves the unit to a target position.</summary>
    /// <param name="destination"></param>
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            Debug.Log("Moving to " + destination);
            agent.SetDestination(destination);
            IsMoving = true;
        }
    }

    /// <summary>Stops the unit's movement.</summary>
    public void StopMovement()
    {
        if (agent != null && agent.isActiveAndEnabled && IsMoving)
        {
            Debug.Log("Stopping movement");
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            IsMoving = false;
        }
    }
    #endregion

    #region Battle mecanics

    /// <summary>Starts the round for the unit, allowing it to engage in combat.</summary>
    public virtual void StartRound()
    {
        RoundStarted = true;
    }

    /// <summary>Applies damage to the unit and checks if it should be dead.</summary>
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            OnUnitDeath?.Invoke();
            // Explosion animation
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>Detects enemies within range and engages them if found.</summary>
    private void DetectEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius/*, detectionMask*/);

        float closestDistanceSqr = float.MaxValue;
        Unit closestEnemy = null;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;

            
            Unit otherUnit = hit.GetComponent<Unit>();

            if (otherUnit == null || knownFriendlies.Contains(otherUnit))
                continue;

            if (otherUnit.team == this.team)
            {
                knownFriendlies.Add(otherUnit);
                continue;
            }

            float distSqr = (otherUnit.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distSqr;
                closestEnemy = otherUnit;
            }
        }
        if (closestEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.transform.position);
            if (distanceToEnemy <= currentAtkRange)
            {
                StopMovement();
                EngageTarget(closestEnemy);
            }
            else
            {
                MoveTo(closestEnemy.transform.position);
            }
        }
    }

    /// <summary>Engages the target unit in combat.</summary>
    public virtual void EngageTarget(Unit target)
    {
        if (currentTarget == target)
            return;
        IsAttacking = true;
        currentTarget = target;
        Debug.Log($"piou piou piou");
        currentTarget.OnUnitDeath += ClearTarget;
        /* TODO */
        // actual attack loop/ coroutine?
    }

    /// <summary>Clears the current target of the unit, stopping any ongoing attack.</summary>
    public virtual void ClearTarget()
    {
        if (currentTarget != null)
            currentTarget.OnUnitDeath -= ClearTarget;

        IsAttacking = false;
        currentTarget = null;
    }

    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (!DebugMode)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentAtkRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    #endregion

}
