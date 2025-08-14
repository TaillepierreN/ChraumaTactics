using System;
using System.Collections.Generic;
using CT.Units.Animations;
using CT.Units.Attacks;
using UnityEngine;
using UnityEngine.AI;


public abstract class Unit : MonoBehaviour
{
    #region Unit Properties
    public bool DebugMode = false;
    public Team team;
    protected Squad squad;
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected UnitType unitType2;
    [SerializeField] protected string unitDescription;
    [SerializeField] protected string unitCost;
    [SerializeField] protected Attack attack;
    public string UnitCost => unitCost;
    /// <summary>
    /// Where the enemy should aim
    /// </summary>
    [SerializeField] protected Transform ownHitbox;

    [Header("Unit Base Stats")]
    [SerializeField] protected int baseHealth;
    [SerializeField] protected int baseAtk;
    [SerializeField] protected float baseMoveSpeed;
    [SerializeField] protected float baseAtkSpeed;
    [SerializeField] protected float baseRange;


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
    [SerializeField] protected bool hasSmoothStop = false;
    [SerializeField] protected float decelerationRate = 2.5f;
    private bool waitingForStop = false;


    [Header("Unit Animation")]
    [SerializeField] protected Animator animatorBody;
    [SerializeField] protected Animator[] animatorWeap;
    [SerializeField] protected TurretAim[] turretAim;
    [SerializeField] private Renderer leftTrackRenderer;
    [SerializeField] private Renderer left2TrackRenderer;
    [SerializeField] private Renderer rightTrackRenderer;
    [SerializeField] private Renderer right2TrackRenderer;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private Material left2TrackMaterial;
    private Material right2TrackMaterial;
    private float leftOffset = 0f;
    private float rightOffset = 0f;


    [Header("Unit Detection settings")]
    [SerializeField] private float detectionRadius = 100f;
    private float detectionInterval = 0.2f;
    private float detectionTimer = 0f;
    /// <summary>Dictionary to keep track of known friendly units to skip repeated Getcomponent check.</summary>
    private Dictionary<GameObject, Unit> knownFriendlies = new();
    /// <summary>Dictionary to keep track of known enemy units to skip repeated GetComponent check.</summary>
    private Dictionary<GameObject, Unit> knownEnemies = new();
    private LayerMask detectionMask = ~0; // all layers for now, TODO: set to only units layer
    private Unit currentTarget = null;



    [Header("State")]
    public bool RoundStarted = false;
    [HideInInspector]
    public bool IsMoving = false;
    [HideInInspector]
    public bool IsAttacking = false;
    [HideInInspector]
    public bool IsDead = false;

    #endregion

    #region Unit Events

    /// <summary>Event triggered when the unit dies.</summary>
    public event Action<Unit> OnUnitDeath;

    #endregion

    #region Unit Getters
    /// <summary>
    /// Gets the current health of the unit.
    public int CurrentHealth => currentHealth;
    public int CurrentAtk => currentAtk;
    public Unit CurrentTarget => currentTarget;
    public Transform Hitbox => ownHitbox;

    #endregion

    #region Unity callbacks
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (leftTrackRenderer != null)
            leftTrackMaterial = leftTrackRenderer.material;
        if (rightTrackRenderer != null)
            rightTrackMaterial = rightTrackRenderer.material;
        if (left2TrackRenderer != null)
            left2TrackMaterial = left2TrackRenderer.material;
        if (right2TrackRenderer != null)
            right2TrackMaterial = right2TrackRenderer.material;
        //spawnPosition = transform.position;
    }

    protected virtual void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!RoundStarted)
            return;

        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            DetectEnemies();
        }

        /*Wheel handling*/
        if (IsMoving && agent.velocity.magnitude > 0.01f)
        {
            leftOffset -= 0.5f * Time.deltaTime;
            rightOffset -= 0.5f * Time.deltaTime;
            if (leftTrackMaterial != null)
                leftTrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, leftOffset));
            if (rightTrackMaterial != null)
                rightTrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, rightOffset));
            if (left2TrackMaterial != null)
                left2TrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, -leftOffset));
            if (right2TrackMaterial != null)
                right2TrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, -rightOffset));
        }

        if (hasSmoothStop && waitingForStop)
        {
            if (agent.velocity.magnitude > 0.01f)
            {
                agent.velocity = Vector3.Lerp(agent.velocity, Vector3.zero, Time.deltaTime * decelerationRate);
            }
            else
            {
                agent.ResetPath();
                waitingForStop = false;
                if (animatorBody != null)
                    animatorBody.SetBool("IsMoving", false);
            }
        }
        if (IsAttacking && currentTarget != null)
{
    float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
    if (dist > currentAtkRange)
    {
        if (attack != null && attack.IsContinuous)
            attack.StopAutoFire();

        MoveTo(currentTarget.transform.position);

    }
}
    }

    private void OnEnable()
    {
        IsDead = false;
    }

    void OnDisable()
    {
        if (!IsDead)
        {
            IsDead = true;
            OnUnitDeath?.Invoke(this);
        }
    }

    #endregion

    #region Stats

    /// <summary>Initializes the unit's stats based on base values.</summary>
    public virtual void Initialize()
    {
        SetCurrentStats(baseHealth, baseAtk, baseMoveSpeed, baseAtkSpeed, baseRange);
        agent.isStopped = false;
        attack?.Initialize(this);
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
        if (animatorBody != null)
            animatorBody.SetFloat("MoveSpeed", currentMoveSpeed / 5f);
        if (animatorWeap != null)
            foreach (Animator weap in animatorWeap)
                if (weap != null)
                    weap.SetFloat("AtkSpeed", currentAtkSpeed);
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
        IsDead = false;
    }
    #endregion

    #region Movement mecanics

    /// <summary>Moves the unit to a target position.</summary>
    /// <param name="destination"></param>
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(destination);
            IsMoving = true;
            if (animatorBody != null)
                animatorBody.SetBool("IsMoving", true);
            agent.isStopped = false;
        }
    }

    /// <summary>Stops the unit's movement, either instant or smoothed.</summary>
    public void StopMovement()
    {
        if (agent != null && agent.isActiveAndEnabled && IsMoving)
        {
            IsMoving = false;
            agent.isStopped = true;
            if (!hasSmoothStop)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                if (animatorBody != null)
                    animatorBody.SetBool("IsMoving", false);
            }
            else
                waitingForStop = true;
        }
    }
    #endregion

    #region Battle mecanics

    /// <summary>Starts the round for the unit, allowing it to engage in combat and move.</summary>
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
            IsDead = true;
            OnUnitDeath?.Invoke(this);
            // Explosion animation
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Detects units within range check with known friendlies and enemies.
    /// if unknown, add them to the correct dictionnary.
    /// Check closest enemy, move to attack range and engage
    /// </summary>
    private void DetectEnemies()
    {
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius/*, detectionMask*/);

        float closestDistanceSqr = float.MaxValue;
        Unit closestEnemy = null;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;

            //skip if friendly and already known
            if (knownFriendlies.TryGetValue(hit.gameObject, out Unit friendUnit))
                continue;

            Unit enemyUnit;

            //if not known enemy, check which team it is on
            // if it is an ally, add to known friendlies
            // if it is an enemy, add to known enemies
            if (!knownEnemies.TryGetValue(hit.gameObject, out enemyUnit))
            {
                enemyUnit = hit.GetComponent<Unit>();

                if (enemyUnit == null)
                    continue;

                if (enemyUnit.team == this.team)
                {
                    knownFriendlies.Add(hit.gameObject, enemyUnit);
                    continue;
                }
                knownEnemies.Add(hit.gameObject, enemyUnit);
            }

            float distSqr = (enemyUnit.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distSqr;
                closestEnemy = enemyUnit;
            }
        }

        if (closestEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.transform.position);
            if (turretAim != null)
                foreach (TurretAim turret in turretAim)
                    if (turret != null)
                        turret.SetLookAtTarget(closestEnemy);
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
        if (animatorWeap != null)
            foreach (Animator weap in animatorWeap)
                if (weap != null)
                    weap.SetBool("IsAttacking", true);
        if (unitType == UnitType.Aerial
        && animatorBody != null
        && Vector3.Distance(transform.position, target.transform.position) < 2)
        {
            Debug.Log("is aerial and attacking");
            animatorBody.SetBool("IsAttacking", true);
        }

        if (attack != null && attack.IsContinuous)
            attack.StartAutoFire(currentTarget);
            
    }

    /// <summary>Clears the current target of the unit, stopping any ongoing attack.</summary>
    public virtual void ClearTarget(Unit unit)
    {
        if (unit != null)
            unit.OnUnitDeath -= ClearTarget;

        if (attack != null && attack.IsContinuous)
            attack.StopAutoFire();

        IsAttacking = false;
        if (animatorWeap != null)
            foreach (Animator weap in animatorWeap)
                if (weap != null)
                    weap.SetBool("IsAttacking", false);
        if (unitType == UnitType.Aerial && animatorBody != null)
            animatorBody.SetBool("IsAttacking", false);
        currentTarget = null;
    }

    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (!DebugMode)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, baseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    #endregion

}

