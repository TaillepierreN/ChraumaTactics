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

    public void SelectCommander()
    {
        //_radioGameplay.GameManager.setChosenCommander(commanderData);
        _radioGameplay.RoundManager.StartGame();
        CommanderChosen?.Invoke();
    }
}
