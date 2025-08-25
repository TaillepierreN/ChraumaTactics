using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "CommanderTest", menuName = "Commanders/Test")]

public abstract class Commander : ScriptableObject
{
    [Header("Commander Info")]
    public string commanderName;
    public Sprite portrait;
    public string description;


    [Header("Starting Units")]
    public GameObject unitPrefab1;
    public string unitName1;
    public GameObject unitPrefab2;
    public string unitName2;
    public Sprite unitIcon1;
    public Sprite unitIcon2;
    public Sprite boostIcon1;
    public Sprite boostIcon2;



    // [Header("Bonuses")]
    // public CommanderBonus bonus1;
    // public CommanderBonus bonus2;

    [Header("Commander Stats")]
    public int playerHealth;

    [Header("Augment")]
    public Augment[] StartingAugment;

    /// <summary>
    /// Called when the commander is selected.
    /// This is where you can spawn units, set stats, etc.
    /// </summary>
    // public abstract void OnSelect(Player player);

    /// <summary>
    /// Apply commander bonuses to the game.
    /// </summary>
    // public abstract void ApplyBonuses(Player player);
}
