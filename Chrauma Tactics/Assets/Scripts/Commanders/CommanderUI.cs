using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CT.Gameplay;
using Unity.Netcode;
using CT.Tools;

public class CommanderUI : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text commanderNameText;
    public TMP_Text descriptionText;
    public TMP_Text unit1NameText;
    public TMP_Text unit2NameText;
    public TMP_Text HPText;
    [SerializeField] private Rd_Gameplay _radioGameplay;
    [SerializeField] private Team team = Team.Player1;
    private Commander commanderData;
    public Image UnitIcon1;
    public Image UnitIcon2;
    public Image BoostIcon1;
    public Image BoostIcon2;

    public Action CommanderChosen;


    void Start()
    {
        team = (NetX.IsListening && !NetX.IsHost) ? Team.Player2 : Team.Player1;
    }

    public void SetCommander(Commander commander)
    {

        commanderData = commander;
        UnitIcon1.sprite = commander.unitIcon1;
        UnitIcon2.sprite = commander.unitIcon2;
        BoostIcon1.sprite = commander.boostIcon1;
        BoostIcon2.sprite = commander.boostIcon2;

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

        _radioGameplay.GameManager.RequestCommanderSelection(commanderData, team);
        if (commanderData.StartingAugment != null && commanderData.StartingAugment.Length > 0)
        {
            foreach (Augment augment in commanderData.StartingAugment)
            {
                if (augment == null) continue;
                _radioGameplay.BoostManager.RegisterAugmentToTeam(team, augment);
            }
        }

        if (NetX.NM && NetX.IsListening)
        {
            PlacementNetwork pn = PlacementNetwork.Instance;
            if (pn != null)
            {
                if (commanderData.unitPrefab1 != null)
                    _radioGameplay.GameManager.GrantVoucherServerRpc(pn.IndexOfUnit(commanderData.unitPrefab1));
                if (commanderData.unitPrefab2 != null)
                    _radioGameplay.GameManager.GrantVoucherServerRpc(pn.IndexOfUnit(commanderData.unitPrefab2));
            }
            if (GameFlowNetwork.Instance != null)
                GameFlowNetwork.Instance.NotifyCommanderSelectedServerRpc(team);
        }
        else
        {
            if (commanderData.unitPrefab1 != null) GiveFreeSquadToPlayer(team, commanderData.unitPrefab1);
            if (commanderData.unitPrefab2 != null) GiveFreeSquadToPlayer(team, commanderData.unitPrefab2);
            _radioGameplay.RoundManager.StartGame();
        }
        CommanderChosen?.Invoke();
    }

    private void GiveFreeSquadToPlayer(Team team, GameObject unitPrefab)
    {
        if (unitPrefab == null) return;
        Player player = _radioGameplay.GameManager.GetPlayerByTeam(team);
        if (player == null) return;
        player.GiveFreeSquadVoucher(unitPrefab);
    }
}
