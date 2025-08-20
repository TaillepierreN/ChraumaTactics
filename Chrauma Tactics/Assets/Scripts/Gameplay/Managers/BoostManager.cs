using System;
using System.Collections.Generic;
using UnityEngine;

public struct StatBoost
{
    public float HpPercent;
    public float AtkPercent;
    public float AtkSpeedPercent;
    public float MoveSpeedPercent;
    public float RangePercent;

    /* Allows chaining: a + b + c + ...
    ex existingBoost with 20% atk + anotherBoost with 20%atk will give newBoost with 40% atk */
    public static StatBoost operator +(StatBoost a, StatBoost b) => new StatBoost
    {
        HpPercent = a.HpPercent + b.HpPercent,
        AtkPercent = a.AtkPercent + b.AtkPercent,
        AtkSpeedPercent = a.AtkSpeedPercent + b.AtkSpeedPercent,
        MoveSpeedPercent = a.MoveSpeedPercent + b.MoveSpeedPercent,
        RangePercent = a.RangePercent + b.RangePercent,
    };
}
public class BoostManager : MonoBehaviour
{
    [SerializeField] private Rd_Gameplay _RdGameplay;

    /// <summary>Per team -> per unit type -> all boost</summary>
    private readonly Dictionary<Team, Dictionary<UnitType, StatBoost>> _boosts
            = new Dictionary<Team, Dictionary<UnitType, StatBoost>>();

    /// <summary>keep augment chosen if needed</summary>
    private readonly Dictionary<Team, List<Augment>> _augmentChosenByTeam
            = new Dictionary<Team, List<Augment>>();

    void Awake()
    {
        _RdGameplay.SetBoostManager(this);

        /*Initialize the team dictionnaries*/
        foreach (Team team in Enum.GetValues(typeof(Team)))
        {
            _boosts[team] = new Dictionary<UnitType, StatBoost>();
            _augmentChosenByTeam[team] = new List<Augment>();
        }
    }

    public void RegisterAugmentToTeam(Team team, Augment augment)
    {
        _augmentChosenByTeam[team].Add(augment);

        StatBoost newBoost = new StatBoost();

        switch (augment.statType)
        {
            case StatType.HP:
                newBoost.HpPercent = augment.bonusValue;
                break;
            case StatType.ATK:
                newBoost.AtkPercent = augment.bonusValue;
                break;
            case StatType.ATKSpeed:
                newBoost.AtkSpeedPercent = augment.bonusValue;
                break;
            case StatType.MovementSpeed:
                newBoost.MoveSpeedPercent = augment.bonusValue;
                break;
            case StatType.Range:
                newBoost.RangePercent = augment.bonusValue;
                break;
        }
        if (_boosts[team].TryGetValue(augment.targetType, out StatBoost prevBoost))
            _boosts[team][augment.targetType] = prevBoost + newBoost;
        else
            _boosts[team][augment.targetType] = newBoost;

        if (!_RdGameplay.GameManager.SpendCredits(AugmentCosts.GetCost(augment.rarity), team))
            Debug.LogWarning($"BoostManager: not enough credits to pay {augment.augmentName} {augment.rarity} for team {team}");

    }

    public StatBoost GetBoostForUnit(Unit unit)
    {
        StatBoost total = default;
        total += GetBoostsForType(unit.team, unit.UnitType);
        total += GetBoostsForType(unit.team, unit.UnitType2);
        return total;
    }

    public StatBoost GetBoostsForType(Team team, UnitType type)
    => _boosts[team].TryGetValue(type, out StatBoost boost) ? boost : default;

    public void ApplyBoostsToArmy(IEnumerable<Squad> army)
    {
        foreach (Squad squad in army)
        {
            foreach (Unit unit in squad.Units)
            {
                StatBoost boost = GetBoostForUnit(unit);
                unit.UpdateBoostedStats(boost);
            }
        }
    }
}
