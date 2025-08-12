using UnityEngine;

[System.Serializable]
public class CommanderBonus
{
    public string bonusName;
    public string description;

    public UnitType targetType;

    public float rangeModifier = 0f;
    public float attackModifier = 0f;
    public float moveSpeedModifier = 0f;
    public float attackSpeedModifier = 0f;
    public float hpModifier = 0f;
}
