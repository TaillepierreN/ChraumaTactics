using System;
using System.Collections;
using System.Collections.Generic;
using CT.Units.Animations;
using CT.Units.Attacks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using CT.Tools;


public abstract class Unit : NetworkBehaviour
{
    #region Unit Properties
    public bool DebugMode = false;
    [ShowIf("DebugMode")]
    public bool DebugStats = false;

    public Team team;
    protected Squad _squad;
    [SerializeField] protected UnitType _unitType;
    [SerializeField] protected UnitType _unitType2;
    [SerializeField] protected string _unitDescription;
    [SerializeField] protected int _unitCost;
    [SerializeField] protected Attack _attack;
    /// <summary>
    /// Where the enemy should aim
    /// </summary>
    [SerializeField] protected Transform _ownHitbox;

    [Header("Unit Base Stats")]
    [SerializeField] protected int _baseHealth = 1000;
    [SerializeField] protected int _baseAtk = 100;
    [SerializeField] protected float _baseMoveSpeed = 20f;
    [SerializeField] protected float _baseAtkSpeed = 20f;
    [SerializeField] protected float _baseRange = 20f;


    [Header("Unit Boosted Stats")]
    // These stats will be modified by bonuses or boosts during the game.
    protected int _boostedHealth;
    protected int _boostedAtk;
    protected float _boostedMoveSpeed;
    protected float _boostedAtkSpeed;
    protected float _boostedRange;


    [Header("Unit Current Stats")]
    protected int _currentHealth;
    protected int _currentAtk;
    protected float _currentMoveSpeed;
    protected float _currentAtkSpeed;
    protected float _currentAtkRange;


    [Header("Unit Spawn Position")]
    /// <summary>The position where the unit spawns at the start of the game or round.</summary>
    public Vector3 SpawnPosition;


    [Header("Unit NavMesh Agent")]
    protected NavMeshAgent _agent;
    [SerializeField] protected bool _hasSmoothStop = false;
    [ShowIf("_hasSmoothStop")]
    [SerializeField] protected float _decelerationRate = 2.5f;
    private bool _waitingForStop = false;


    [Header("Unit Animation")]
    [SerializeField] protected Animator _animatorBody;
    [SerializeField] protected NetworkAnimator _netAnimatorBody;
    [SerializeField] protected Animator[] _animatorWeap;
    [SerializeField] protected TurretAim[] _turretAim;
    [SerializeField] private Renderer _leftTrackRenderer;
    [SerializeField] private Renderer _left2TrackRenderer;
    [SerializeField] private Renderer _rightTrackRenderer;
    [SerializeField] private Renderer _right2TrackRenderer;
    private ContinuousNetProxy _cProxy;
    private Material _leftTrackMaterial;
    private Material _rightTrackMaterial;
    private Material _left2TrackMaterial;
    private Material _right2TrackMaterial;
    private float _leftOffset = 0f;
    private float _rightOffset = 0f;
    static readonly int HASH_IsMoving = Animator.StringToHash("IsMoving");
    static readonly int HASH_IsAttacking = Animator.StringToHash("IsAttacking");
    static readonly int HASH_MoveSpeed = Animator.StringToHash("MoveSpeed");
    static readonly int HASH_AtkSpeed = Animator.StringToHash("AtkSpeed");


    [Header("Unit Audio")]
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected AudioClip _moveClip;


    [Header("Unit Detection settings")]
    [SerializeField] private float _detectionRadius = 100f;
    private float _detectionInterval = 0.2f;
    private float _detectionTimer = 0f;
    /// <summary>Dictionary to keep track of known friendly units to skip repeated Getcomponent check.</summary>
    private Dictionary<GameObject, Unit> _knownFriendlies = new();
    /// <summary>Dictionary to keep track of known enemy units to skip repeated GetComponent check.</summary>
    private Dictionary<GameObject, Unit> _knownEnemies = new();
    /// <summary>Dictionary to keep track of know untargetable unit to skip if can't hit them</summary>
    private Dictionary<GameObject, Unit> _knownUntargetable = new();
    private readonly Collider[] _hits = new Collider[258];
    private LayerMask _detectionMask;
    private Unit _currentTarget = null;
    private bool _targetHasMovedAway = false;

    [Header("Aiming")]
    [SerializeField] private bool _requierFacingForAttack = false;
    [SerializeField] private bool _canTargetFlying;
    [SerializeField] private bool _aerialOnly = false;

    [ShowIf("_requierFacingForAttack")]
    [SerializeField, Range(0f, 90f)] private float _facingConeDegree = 32f;
    [ShowIf("_requierFacingForAttack")]
    [SerializeField] private float _facingEpsilonDeg = 1.5f;
    private float _minFacingDot = 0.85f;
    private Coroutine _alignRoutine;


    [Header("State")]
    public bool RoundStarted = false;
    [HideInInspector]
    public bool IsMoving = false;
    [HideInInspector]
    public bool IsAttacking = false;
    [HideInInspector]
    public bool IsDead = false;

    [Header("UI")]
    [SerializeField] private CanvasGroup _hpBarCanvas;
    [SerializeField] private Slider _hpBar;
    [SerializeField] private float _hpBarSpeed = 100f;
    [SerializeField] private float _delayBeforeFade = 5f;
    private Coroutine _hpDisplayCoroutine;
    private float _barTargetHp;
    private float _delayUntil = -1f;
    /// <summary>tolerance threshold (really close to 0)</summary>
    const float EPSILON = 0.001f;

    [Header("Network")]
    private NetworkVariable<int> _netHealth = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkObject no;

    #endregion

    #region Unit Events

    /// <summary>Event triggered when the unit dies.</summary>
    public event Action<Unit> OnUnitDeath;

    #endregion

    #region Unit Getters
    /// <summary>
    /// Gets the current health of the unit.
    public UnitType UnitType => _unitType;
    public UnitType UnitType2 => _unitType2;
    public int CurrentHealth => _currentHealth;
    public int CurrentAtk => _currentAtk;
    public Unit CurrentTarget => _currentTarget;
    public Transform Hitbox => _ownHitbox;
    public int UnitCost => _unitCost;
    public bool IsTargetable => !IsDead && gameObject.activeInHierarchy;


    #endregion

    #region Unity callbacks
    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
        if (_leftTrackRenderer != null)
            _leftTrackMaterial = _leftTrackRenderer.material;
        if (_rightTrackRenderer != null)
            _rightTrackMaterial = _rightTrackRenderer.material;
        if (_left2TrackRenderer != null)
            _left2TrackMaterial = _left2TrackRenderer.material;
        if (_right2TrackRenderer != null)
            _right2TrackMaterial = _right2TrackRenderer.material;
        SpawnPosition = transform.position;
        _minFacingDot = DotFromDegrees(_facingConeDegree);
        if (_aerialOnly)
            _canTargetFlying = true;
        _detectionMask = LayerMask.GetMask("Units");
    }

    protected virtual void Start()
    {
        Initialize();
        if (_attack.IsContinuous)
            _cProxy = GetComponent<ContinuousNetProxy>();
    }

    private void Update()
    {
        if (!RoundStarted || IsDead)
            return;

        _detectionTimer += Time.deltaTime;
        if (_detectionTimer >= _detectionInterval)
        {
            _detectionTimer = 0f;
            DetectEnemies();
        }


        /*Wheel handling*/
        if (IsMoving && _agent.velocity.magnitude > 0.01f)
        {
            _leftOffset -= 0.5f * Time.deltaTime;
            _rightOffset -= 0.5f * Time.deltaTime;
            if (_leftTrackMaterial != null)
                _leftTrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, _leftOffset));
            if (_rightTrackMaterial != null)
                _rightTrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, _rightOffset));
            if (_left2TrackMaterial != null)
                _left2TrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, -_leftOffset));
            if (_right2TrackMaterial != null)
                _right2TrackMaterial.SetTextureOffset("_BaseMap", new Vector2(0, -_rightOffset));
        }

        if (_hasSmoothStop && _waitingForStop)
        {
            if (_agent.velocity.magnitude > 0.01f)
            {
                _agent.velocity = Vector3.Lerp(_agent.velocity, Vector3.zero, Time.deltaTime * _decelerationRate);
            }
            else
            {
                _agent.ResetPath();
                _waitingForStop = false;
                PlayMoveSound(false);
                if (_animatorBody != null)
                    _animatorBody.SetBool(HASH_IsMoving, false);
            }
        }
        if (IsAttacking && _currentTarget != null)
        {
            if (!_currentTarget.IsTargetable)
            {
                ClearTarget(_currentTarget);
                return;
            }
            // check if enemy is still in atk range,otherwise move to it
            float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);
            if (dist > _currentAtkRange)
            {
                if (_attack != null && _attack.IsContinuous)
                    _attack.StopAutoFire();

                _targetHasMovedAway = true;

                if (_animatorWeap != null)
                    foreach (Animator weap in _animatorWeap)
                        if (weap != null)
                            weap.SetBool(HASH_IsAttacking, false);

                if (_unitType == UnitType.Aerial && _animatorBody != null)
                    _animatorBody.SetBool(HASH_IsAttacking, false);

                MoveTo(_currentTarget.transform.position);
            }
            else if (_targetHasMovedAway)
            {
                if (_requierFacingForAttack && !IsFacing(_currentTarget.transform))
                {
                    if (_alignRoutine != null)
                        StopCoroutine(_alignRoutine);
                    _alignRoutine = StartCoroutine(AlignAndEngage(_currentTarget));
                }
                else
                {
                    if (_attack != null && _attack.IsContinuous)
                        StartBeamAttack();

                    if (_animatorWeap != null)
                        foreach (Animator weap in _animatorWeap)
                            if (weap != null)
                                weap.SetBool(HASH_IsAttacking, true);

                    if (_unitType == UnitType.Aerial
                    && _animatorBody != null
                    && Vector3.Distance(transform.position, _currentTarget.transform.position) < 2)
                    {
                        Debug.Log("is aerial and attacking");
                        _animatorBody.SetBool(HASH_IsAttacking, true);
                    }
                    _targetHasMovedAway = false;
                }
            }
        }
    }

    private void OnEnable()
    {
        IsDead = false;
    }

    void OnDisable()
    {
        _hpDisplayCoroutine = null;
        if (!IsDead)
        {
            IsDead = true;
            OnUnitDeath?.Invoke(this);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _netHealth.OnValueChanged += OnNetHealthChanged;

        if (IsClient)
        {
            if (_netHealth.Value > 0)
            {
                _hpBar.maxValue = (_hpBar.maxValue <= 0) ? _baseHealth : _hpBar.maxValue;
                OnNetHealthChanged(0, _netHealth.Value);
            }
        }

        if (IsServer)
            _netHealth.Value = _currentHealth;
    }

    public override void OnNetworkDespawn()
    {
        _netHealth.OnValueChanged -= OnNetHealthChanged;
        base.OnNetworkDespawn();
    }

    #endregion

    #region Stats

    /// <summary>Initializes the unit's stats based on base values.</summary>
    public virtual void Initialize()
    {
        SetCurrentStats(_baseHealth, _baseAtk, _baseMoveSpeed, _baseAtkSpeed, _baseRange);
        _hpBarCanvas.alpha = 0;
        _agent.isStopped = false;
        _attack?.Initialize(this);
        if (NetX.InSession)
            TryGetComponent<NetworkObject>(out no);
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
        _squad = newSquad;
    }

    /// <summary>Sets the current stats of the unit.</summary>
    /// <param name="health"></param>
    /// <param name="atk"></param>
    /// <param name="moveSpeed"></param>
    /// <param name="atkSpeed"></param>
    /// <param name="range"></param>
    public virtual void SetCurrentStats(int health, int atk, float moveSpeed, float atkSpeed, float range)
    {
        if (team == Team.Player1 && DebugMode && DebugStats)
        {
            Debug.Log($"Setting stat for {gameObject.name}");
            Debug.Log($"Health: {_baseHealth} -> {health}");
            Debug.Log($"Atk: {_baseAtk} -> {atk}");
            Debug.Log($"MoveSpeed: {_baseMoveSpeed} -> {moveSpeed}");
            Debug.Log($"AtkSpeed: {_baseAtkSpeed} -> {atkSpeed}");
            Debug.Log($"Range: {_baseRange} -> {range}");
        }
        _currentHealth = health;
        _currentAtk = atk;
        _currentMoveSpeed = moveSpeed;
        _currentAtkSpeed = atkSpeed;
        _currentAtkRange = range;
        _agent.speed = _currentMoveSpeed;
        if (_animatorBody != null)
            _animatorBody.SetFloat(HASH_MoveSpeed, _currentMoveSpeed / 5f);
        if (_animatorWeap != null)
            foreach (Animator weap in _animatorWeap)
                if (weap != null)
                    weap.SetFloat(HASH_AtkSpeed, _currentAtkSpeed);
        _hpBar.maxValue = _currentHealth;
        _hpBar.value = _currentHealth;
        if (IsServer)
            _netHealth.Value = _currentHealth;
    }

    /// <summary>
    /// Updates the unit's stats based on the given stat boosts
    /// </summary>
    /// <param name="boosts"></param>
    public virtual void UpdateBoostedStats(StatBoost boosts)
    {
        float hpMultiplicator = 1f + boosts.HpPercent * 0.01f;
        float atkMultiplicator = 1f + boosts.AtkPercent * 0.01f;
        float aspdMultiplicator = 1f + boosts.AtkSpeedPercent * 0.01f;
        float mspMultiplicator = 1f + boosts.MoveSpeedPercent * 0.01f;
        float rngMultiplicator = 1f + boosts.RangePercent * 0.01f;

        _boostedHealth = Mathf.RoundToInt(_baseHealth * hpMultiplicator);
        _boostedAtk = Mathf.RoundToInt(_baseAtk * atkMultiplicator);
        _boostedAtkSpeed = _baseAtkSpeed * aspdMultiplicator;
        _boostedMoveSpeed = _baseMoveSpeed * mspMultiplicator;
        _boostedRange = _baseRange * rngMultiplicator;

        SetCurrentStats(_boostedHealth, _boostedAtk, _boostedMoveSpeed, _boostedAtkSpeed, _boostedRange);
    }

    /// <summary>Resets the unit's stats to the boosted values and position .Call at game round end</summary>
    public virtual void ResetStats()
    {
        SetCurrentStats(_baseHealth, _baseAtk, _baseMoveSpeed, _baseAtkSpeed, _baseRange);
        if (_agent && _agent.isActiveAndEnabled)
            _agent.Warp(SpawnPosition);
        else
            transform.position = SpawnPosition;
    }

    /// <summary>
    /// Coroutine to display hp bar of unit
    /// set slider to go down faster if more hp is missing
    /// </summary>
    /// <returns></returns>
    IEnumerator DisplayHealth()
    {
        if (_hpBarCanvas.alpha < 1f)
            yield return Fade(_hpBarCanvas, 1f, 0.2f);

        while (true)
        {
            float dt = Time.unscaledDeltaTime;
            float diff = Mathf.Abs(_hpBar.value - _barTargetHp);

            if (diff > EPSILON)
            {
                // bigger gap ⇒ bigger step
                float step = (_hpBarSpeed + 10f * diff) * dt;
                _hpBar.value = Mathf.MoveTowards(_hpBar.value, _barTargetHp, step);
            }
            else
            {
                _hpBar.value = _barTargetHp;
            }

            diff = Mathf.Abs(_hpBar.value - _barTargetHp);
            if (diff <= EPSILON && Time.unscaledTime >= _delayUntil)
            {
                yield return Fade(_hpBarCanvas, 0f, 0.15f);
                _hpDisplayCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private void OnNetHealthChanged(int oldValue, int newValue)
    {
        _barTargetHp = Mathf.Clamp(newValue, _hpBar.minValue, _hpBar.maxValue);
        _delayUntil = Time.unscaledTime + _delayBeforeFade;

        if (_hpDisplayCoroutine == null)
            _hpDisplayCoroutine = StartCoroutine(DisplayHealth());
    }

    #endregion

    #region Movement mecanics

    /// <summary>Moves the unit to a target position.</summary>
    /// <param name="destination"></param>
    public void MoveTo(Vector3 destination)
    {
        if (_agent != null && _agent.isActiveAndEnabled)
        {
            _agent.SetDestination(destination);
            IsMoving = true;
            PlayMoveSound();
            if (_animatorBody != null)
                _animatorBody.SetBool(HASH_IsMoving, true);
            _agent.isStopped = false;
        }
    }

    private void PlayMoveSound(bool shouldPlay = true)
    {
        if (_audioSource != null && _moveClip != null)
            if (shouldPlay && !_audioSource.isPlaying)
            {
                _audioSource.clip = _moveClip;
                _audioSource.time = UnityEngine.Random.Range(0f, _moveClip.length - 0.1f);
                _audioSource.loop = true;
                _audioSource.Play();
            }
            else if (!shouldPlay && _audioSource.isPlaying)
            {
                _audioSource.loop = false;
                _audioSource.Stop();
            }

    }

    /// <summary>Stops the unit's movement, either instant or smoothed.</summary>
    public void StopMovement()
    {
        if (_agent != null && _agent.isActiveAndEnabled && IsMoving)
        {
            IsMoving = false;
            _agent.isStopped = true;
            if (!_hasSmoothStop)
            {
                _agent.ResetPath();
                PlayMoveSound(false);
                _agent.velocity = Vector3.zero;
                if (_animatorBody != null)
                    _animatorBody.SetBool(HASH_IsMoving, false);
            }
            else
                _waitingForStop = true;
        }
    }
    #endregion

    #region Battle mecanics

    /// <summary>Starts the round for the unit, allowing it to engage in combat and move.</summary>
    public virtual void StartRound()
    {
        _knownFriendlies.Clear();
        _knownEnemies.Clear();
        _knownUntargetable.Clear();
        _currentTarget = null;
        _targetHasMovedAway = false;
        RoundStarted = true;
        //UpdateBoostedStats()
    }

    /// <summary>End the round, reset stats and position/// </summary>
    public virtual void EndRound()
    {
        RoundStarted = false;

        StopMovement();
        PlayMoveSound(false);
        if (CurrentTarget != null)
            ClearTarget(CurrentTarget);

        (_attack as Ballistic)?.ClearProjectiles();

        _knownUntargetable.Clear();

        if (IsDead)
            IsDead = false;
        if (NetX.InSession)
            _netAnimatorBody?.SetTrigger(Animator.StringToHash("Revive"));
        else
            _animatorBody?.SetTrigger(Animator.StringToHash("Revive"));
        _waitingForStop = false;
        ResetStats();
    }

    /// <summary>Applies damage to the unit, call for hp bar update and checks if it should be dead.</summary>
    public virtual void TakeDamage(int damage)
    {
        if (NetX.InSession && !IsServer)
            return;

        if (IsDead)
            return;

        _currentHealth = Mathf.Max(0, _currentHealth - damage);

        if (NetX.InSession)
            _netHealth.Value = _currentHealth;

        _barTargetHp = Mathf.Clamp(_currentHealth, _hpBar.minValue, _hpBar.maxValue);
        _delayUntil = Time.unscaledTime + _delayBeforeFade;

        if (_hpDisplayCoroutine == null)
            _hpDisplayCoroutine = StartCoroutine(DisplayHealth());

        if (_currentHealth <= 0)
        {
            IsDead = true;
            StopMovement();
            if (_currentTarget != null) ClearTarget(_currentTarget);
            RoundStarted = false;

            if (NetX.InSession)
                _netAnimatorBody.SetTrigger(Animator.StringToHash("IsDead"));
            else
                _animatorBody.SetTrigger(Animator.StringToHash("IsDead"));
            OnUnitDeath?.Invoke(this);
            _hpBarCanvas.alpha = 0f;
            // Explosion animation
        }
    }


    /// <summary>
    /// Detects units within range check with known friendlies untargetable and enemies.
    /// if unknown, add them to the correct dictionnary.
    /// Check closest enemy, move to attack range and engage
    /// </summary>
    private void DetectEnemies()
    {
        if (IsDead)
            return;
        if (_currentTarget != null)
        {
            if (_currentTarget.IsTargetable)
                return;
            ClearTarget(_currentTarget);
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectionRadius, _hits, _detectionMask);

        float closestDistanceSqr = float.MaxValue;
        Unit closestEnemy = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hits[i];

            if (!hit)
                continue;

            GameObject hitGo = hit.gameObject;

            if (hitGo == this.gameObject)
                continue;

            //skip if friendly and already known
            if (_knownFriendlies.ContainsKey(hitGo) || _knownUntargetable.ContainsKey(hitGo))
                continue;

            if (_knownEnemies.TryGetValue(hitGo, out Unit enemyUnit))
            {
                if (!enemyUnit.IsTargetable)
                {
                    _knownEnemies.Remove(hitGo);
                    _knownUntargetable[hitGo] = enemyUnit;
                    continue;
                }
            }
            //if not known enemy, check which team it is on
            // if it is an ally, add to known friendlies
            // if it is an enemy, add to known enemies
            else
            {
                enemyUnit = hitGo.GetComponentInParent<Unit>();
                if (enemyUnit == null) continue;

                if (!enemyUnit.IsTargetable)
                {
                    _knownUntargetable.TryAdd(hitGo, enemyUnit);
                    continue;
                }

                if (enemyUnit.team == this.team)
                {
                    _knownFriendlies.TryAdd(hitGo, enemyUnit);
                    continue;
                }
                if (_aerialOnly && enemyUnit._unitType != UnitType.Aerial)
                {
                    _knownUntargetable.TryAdd(hitGo, enemyUnit);
                    continue;
                }
                if (!_canTargetFlying && enemyUnit._unitType == UnitType.Aerial)
                {
                    _knownUntargetable.TryAdd(hitGo, enemyUnit);
                    continue;
                }

                _knownEnemies.TryAdd(hitGo, enemyUnit);
            }

            float distSqr = (enemyUnit.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distSqr;
                closestEnemy = enemyUnit;
            }
        }

        float atkRangeSqr = _currentAtkRange * _currentAtkRange;
        if (closestEnemy != null)
        {
            if (_turretAim != null)
                foreach (TurretAim turret in _turretAim)
                    if (turret != null)
                        turret.SetLookAtTarget(closestEnemy);

            if (closestDistanceSqr <= atkRangeSqr)
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
        if (target == null || !target.IsTargetable)
            return;
        if (_currentTarget == target)
            return;

        _currentTarget = target;
        IsAttacking = false;
        _currentTarget.OnUnitDeath += ClearTarget;

        if (_requierFacingForAttack && Vector3.Distance(transform.position, target.transform.position) <= _currentAtkRange && !IsFacing(target.transform))
        {
            if (_alignRoutine != null)
                StopCoroutine(_alignRoutine);
            _alignRoutine = StartCoroutine(AlignAndEngage(target));
        }
        else
            BeginFiring(target);
    }

    /// <summary>
    /// Turn the unit toward the target and begin firing
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator AlignAndEngage(Unit target)
    {
        if (_agent == null || target == null) yield break;

        bool prevUpdateRot = _agent.updateRotation;
        _agent.updateRotation = false;
        _agent.isStopped = true;

        while (target != null && target.gameObject.activeInHierarchy)
        {
            if (IsDead) yield break;

            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (dist > _currentAtkRange)
            {
                _agent.updateRotation = prevUpdateRot;
                MoveTo(target.transform.position);
                _alignRoutine = null;
                yield break;
            }

            if (IsFacing(target.transform))
                break;

            Vector3 look = target.transform.position; look.y = transform.position.y;
            Quaternion wanted = Quaternion.LookRotation((look - transform.position).normalized);
            float step = Mathf.Max(1f, _agent.angularSpeed) * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, wanted, step);

            if (Quaternion.Angle(transform.rotation, wanted) <= _facingEpsilonDeg)
                break;

            yield return null;
        }

        _agent.updateRotation = prevUpdateRot;
        BeginFiring(target);
        _alignRoutine = null;
    }

    /// <summary>
    /// Start the shooting animation / damage dealing
    /// </summary>
    /// <param name="target"></param>
    private void BeginFiring(Unit target)
    {
        if (IsDead)
            return;
        IsAttacking = true;
        if (_animatorWeap != null)
            foreach (Animator weap in _animatorWeap)
                if (weap) weap.SetBool(HASH_IsAttacking, true);

        if (_unitType == UnitType.Aerial && _animatorBody != null &&
            Vector3.Distance(transform.position, target.transform.position) < 2)
            _animatorBody.SetBool(HASH_IsAttacking, true);

        if (_attack != null && _attack.IsContinuous)
        {
            StartBeamAttack();
        }
    }

    private void StartBeamAttack()
    {
        if (NetX.InSession && _cProxy)
            _cProxy.StartAllBeamsNetworked(_currentTarget);
        else
            _attack.StartAutoFire(_currentTarget);
    }

    /// <summary>Clears the current target of the unit, stopping any ongoing attack.</summary>
    public virtual void ClearTarget(Unit unit)
    {
        if (unit != null)
            unit.OnUnitDeath -= ClearTarget;

        if (_alignRoutine != null)
        {
            StopCoroutine(_alignRoutine);
            _alignRoutine = null;
        }

        if (_agent != null)
            _agent.updateRotation = true;

        if (_attack != null && _attack.IsContinuous)
        {
            if (NetX.InSession && _cProxy)
                _cProxy.StopAllBeamsNetworked();
            else
                _attack.StopAutoFire();
        }

        if (_animatorWeap != null)
            foreach (Animator weap in _animatorWeap)
                if (weap != null)
                    weap.SetBool(HASH_IsAttacking, false);

        if (_unitType == UnitType.Aerial && _animatorBody != null)
            _animatorBody.SetBool(HASH_IsAttacking, false);

        _targetHasMovedAway = false;
        IsAttacking = false;
        _currentTarget = null;
    }

    #endregion

    #region Helpers
    /// <summary>
    /// Check if inside the cone in front of the unit
    /// </summary>
    /// <param name="t">target</param>
    /// <returns></returns>
    private bool IsFacing(Transform t)
    {
        Vector3 vectorToTarget = t.position - transform.position;
        vectorToTarget.y = 0f;
        /*is facing if on top of unit*/
        if (vectorToTarget.sqrMagnitude < 0.0001f)
            return true;
        float dot = Vector3.Dot(transform.forward, vectorToTarget.normalized);
        return dot >= _minFacingDot;
    }

    /// <summary>
    /// turn degrees into a dot product, used fo check if facing
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    private float DotFromDegrees(float degrees) => Mathf.Cos(degrees * Mathf.Deg2Rad);

    /// <summary>
    /// Fade in or out to target alpha
    /// </summary>
    /// <param name="targetAlpha">float alpha target</param>
    /// <returns></returns>
    private IEnumerator Fade(CanvasGroup targetGroup, float targetAlpha, float fadeDuration)
    {
        float start = targetGroup.alpha;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            targetGroup.alpha = Mathf.Lerp(start, targetAlpha, time / fadeDuration);
            yield return null;
        }
        targetGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Grant aerial targeting to unit
    /// </summary>
    public void GrantAntiAir()
    {
        if (!_canTargetFlying)
        {
            _canTargetFlying = true;
            _knownUntargetable.Clear();
        }
    }


    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (!DebugMode)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _baseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
    #endregion

}

