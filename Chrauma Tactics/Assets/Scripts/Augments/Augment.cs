using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public enum AugmentRarity
{
    Silver,
    Gold,
    Quantum
}

public enum StatType
{
    HP,
    ATK,
    ATKSpeed,
    MovementSpeed,
    Range
}


[CreateAssetMenu(fileName = "NewAugment", menuName = "Augments/Augment")]
public class Augment : ScriptableObject
{
    [Header("Basic Info")]
    public string augmentName;
    public Sprite icon;
    public AugmentRarity rarity;
    public UnitType targetType;
    public StatType statType;

    [Header("Effect")]
    [Tooltip("Value in percentage, e.g. 12 for 12%")]
    public float bonusValue;

}

public static class AugmentCosts
{
    public static readonly Dictionary<AugmentRarity, int> CostByRarity = new Dictionary<AugmentRarity, int>
    {
        { AugmentRarity.Silver, 50 },
        { AugmentRarity.Gold, 100 },
        { AugmentRarity.Quantum, 150 }
    };

    public static int GetCost(AugmentRarity rarity)
    {
        return CostByRarity[rarity];
    }
}
