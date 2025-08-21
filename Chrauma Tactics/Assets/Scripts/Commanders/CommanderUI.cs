using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CommanderUI : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text commanderNameText;
    public TMP_Text descriptionText;
    public TMP_Text unit1NameText;
    public TMP_Text unit2NameText;
    public TMP_Text HPText;
    [SerializeField] private Rd_Gameplay _radioGameplay;
    [SerializeField] private Team team;
    private Commander commanderData;

    public Action CommanderChosen;


    public void SetCommander(Commander commander)
    {
        commanderData = commander;
        HPText.text = commander.playerHealth.ToString();
        descriptionText.text = commander.description;
        unit1NameText.text = commander.unitName1;
        unit2NameText.text = commander.unitName2;

        commanderNameText.text = commander.commanderName;
        portraitImage.sprite = commander.portrait;
    }

    /// <summary>
    /// Onclick to select commander
    /// </summary>
    public void SelectCommander()
    {
        _radioGameplay.GameManager.SetChosenCommander(commanderData);
        if (commanderData.StartingAugment != null && commanderData.StartingAugment.Length > 0)
        {
            foreach (Augment augment in commanderData.StartingAugment)
            {
                if (augment == null) continue;
                _radioGameplay.BoostManager.RegisterAugmentToTeam(team, augment);
            }
        }
        _radioGameplay.RoundManager.StartGame();
        CommanderChosen?.Invoke();
    }
}
