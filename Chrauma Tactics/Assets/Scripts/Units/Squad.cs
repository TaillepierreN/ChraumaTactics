using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool DebugMode = false;
    /// <summary>If true, the squad will spawn units when space bar is pressed.(need debugmode)</summary>
    public bool startSpawnDebug = false;


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
    private List<Unit> units = new List<Unit>();


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
            Vector3 spawnPos = formation[i];

            GameObject newUnitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity, this.transform);
            Unit unit = newUnitObj.GetComponent<Unit>();

            if (unit != null)
            {
                unit.SetTeam(team);
                unit.spawnPosition = spawnPos;
                unit.Initialize();
                unit.SetSquad(this);
                units.Add(unit);
            }
        }
    }

    #endregion

    #region Squad Management

    /// <summary>Moves all units in the squad to the specified destination.</summary>
    /// <param name="destination"></param>
    public void MoveTo(Vector3 destination)
    {
        foreach (Unit unit in units)
        {
            unit.MoveTo(destination);
        }
    }

    #endregion

    #region Gameplay Methods

    /// <summary>Starts the round for all units in the squad, allowing them to perform their actions.</summary>
    public void StartRound()
    {
        foreach (Unit unit in units)
        {
            unit.StartRound();
        }
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
            Debug.Log($"Spawn Positions Count: {SpawnPositions.Count}");
            SpawnUnit();
        }
    }

    #endregion
}
