using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugArmy : MonoBehaviour
{
    [SerializeField] private Unit[] army;
    [SerializeField] private Team team;
    /// <summary>
    /// set debug mode for all childrens
    /// </summary>
    [SerializeField] private bool allDebug;

    public event Action StartWar;

    void Awake()
    {
        StartWar += StartTheWar;
    }
    void OnDisable()
    {
        StartWar -= StartTheWar;
    }

	void Start()
	{
        BuildArmy();
	}

    /// <summary>
    /// populate the array with children to make an army
    /// enable or disable debugmode
    /// </summary>
    private void BuildArmy()
    {
        var list = new List<Unit>(transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Unit unit = child.GetComponent<Unit>();
            if (unit != null)
                list.Add(unit);
            if (allDebug)
                unit.DebugMode = true;
            else
                unit.DebugMode = false;
        }
        army = list.ToArray();
        Debug.Log($"Army team {team} is ready");
    }

    /// <summary>
    /// act as a start round for all children
    /// </summary>
    [ContextMenu("Start the war")]
    public void StartTheWar()
    {
        foreach (var soldier in army)
        {
            soldier.team = team;
            soldier.StartRound();
        }
        Debug.Log($"Team {team} is engaging");
    }
}
