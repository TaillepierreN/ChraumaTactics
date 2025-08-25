using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Squad : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool DebugMode = false;
    /// <summary>If true, the squad will spawn units when space bar is pressed.(need debugmode)</summary>
    public bool startSpawnDebug = false;

    [Header("Refs")]
    [SerializeField] private Rd_Gameplay _radioGameplay;


    [Header("Squad Settings")]
    public Team team;
    /// <summary>
    /// number of units to spawn in the squad.
    /// base on unit size, max 5
    /// </summary>
    public int nbrOfUnits = 1;
    /// <summary>Prefab of the unit to spawn in the squad.</summary>
    public GameObject unitPrefab;
    /// <summary>
    /// List of spawn positions for the units in the squad.
    /// 2     3
    ///    1   
    /// 4     5
    /// </summary>
    public List<Transform> SpawnPositions = new List<Transform>();
    [Header("Squad Properties")]
    /// <summary>The list of units in the squad.</summary>
    private List<Unit> _units = new List<Unit>();
    public int nbrOfDeadUnit = 0;
    public IReadOnlyList<Unit> Units => _units;


    void Start()
    {
        _radioGameplay.GameManager.SetSquadPhase += SetUnitAction;
    }

    void OnDisable()
    {
        _radioGameplay.GameManager.SetSquadPhase -= SetUnitAction;

    }

    #region Squad creation
    /// <summary>Spawns the units in the squad at their designated spawn positions.</summary>
    // public void SpawnUnit()
    // {
    //     for (int i = 0; i < nbrOfUnits; i++)
    //     {
    //         GameObject newUnitObj = Instantiate(unitPrefab, SpawnPositions[i]);
    //         Unit unit = newUnitObj.GetComponent<Unit>();
    //         if (unit != null)
    //         {
    //             unit.SetTeam(team);
    //             unit.spawnPosition = SpawnPositions[i].position;
    //             unit.Initialize();
    //             unit.SetSquad(this);
    //             units.Add(unit);
    //         }
    //     }
    // }

    public void SpawnUnit()
    {
        List<Vector3> formation = SquadFormationPresets.GetFormation(nbrOfUnits);

        if (formation == null || formation.Count < nbrOfUnits)
        {
            Debug.LogError($"Pas de formation disponible pour {nbrOfUnits} unités.");
            return;
        }

        for (int i = 0; i < nbrOfUnits; i++)
        {
            Vector3 desiredWorldPos = transform.TransformPoint(formation[i]);

            TrySnapToNavMesh(desiredWorldPos, out Vector3 spawnPos);

            GameObject newUnitObj = Instantiate(unitPrefab, spawnPos, transform.rotation, transform);

            NavMeshAgent agent = newUnitObj.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (!agent.enabled)
                    agent.enabled = true;
                agent.Warp(spawnPos);
            }

            Unit unit = newUnitObj.GetComponent<Unit>();

            if (unit != null)
            {
                unit.SetTeam(team);
                unit.SpawnPosition = newUnitObj.transform.position;
                unit.Initialize();
                unit.SetSquad(this);
                _units.Add(unit);
            }
        }
        _radioGameplay.GameManager.AddToArmy(team, this);
    }

    #endregion

    #region Squad Management

    /// <summary>Moves all units in the squad to the specified destination.</summary>
    /// <param name="destination"></param>
    public void MoveTo(Vector3 destination)
    {
        foreach (Unit unit in _units)
        {
            unit.MoveTo(destination);
        }
    }

    #endregion

    #region Gameplay Methods

    /// <summary>Starts the round for all units in the squad, allowing them to perform their actions.</summary>
    public void SetUnitAction(RoundPhase phase)
    {
        switch (phase)
        {
            case RoundPhase.Combat:
                foreach (Unit unit in _units)
                    unit.StartRound();
                nbrOfDeadUnit = 0;
                break;

            case RoundPhase.PostCombat:
                foreach (Unit unit in _units)
                {
                    if (!unit.gameObject.activeSelf)
                    {
                        nbrOfDeadUnit++;
                        unit.gameObject.SetActive(true);
                    }
                    unit.EndRound();
                }
                break;

            default:
                break;
        }
    }

    #endregion

    #region Helper
    /// <summary>
    /// Snap to navmesh, avoid error when placing agent
    /// </summary>
    /// <param name="desired"></param>
    /// <param name="snapped"></param>
    /// <param name="maxDist"></param>
    /// <returns></returns>
    private bool TrySnapToNavMesh(Vector3 desired, out Vector3 snapped, float maxDist = 2f)
    {
        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, maxDist, NavMesh.AllAreas))
        {
            snapped = hit.position;
            return true;
        }
        snapped = desired;
        return false;
    }
    #endregion

    #region Debug Methods

    void Update()
    {
        if (DebugMode)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DebugSpawn();
            }
        }
    }

    /// <summary>Debug method to spawn units in the squad.</summary>
    public void DebugSpawn()
    {
        if (startSpawnDebug)
        {
            Debug.Log("Debug Spawn Units");
            Debug.Log($"Team: {team}, Number of Units: {nbrOfUnits}");
            SpawnUnit();
        }
    }

    #endregion
}
