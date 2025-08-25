using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "CommanderTest", menuName = "Commanders/Test")]

public abstract class Commander : ScriptableObject
{
    [Header("Commander Info")]
    [SerializeField] public string commanderName;
    [SerializeField] public Sprite portrait;
    [SerializeField] public string description;


    [Header("Starting Units")]
    [SerializeField] public GameObject unitPrefab1;
    [SerializeField] public string unitName1;
    [SerializeField] public GameObject unitPrefab2;
    [SerializeField] public string unitName2;
    [SerializeField] public Sprite unitIcon1;
    [SerializeField] public Sprite unitIcon2;
    [SerializeField] public Sprite boostIcon1;
    [SerializeField] public Sprite boostIcon2;



    // [Header("Bonuses")]
    // public CommanderBonus bonus1;
    // public CommanderBonus bonus2;

    [Header("Commander Stats")]
    [SerializeField] public int playerHealth;

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
