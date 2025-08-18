using CT.Gameplay;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rd_Gameplay _radioGameplay;
    private RoundManager _roundManager;


    [Header("Player stats")]
    public Player player1;
    public Player player2;

    public Action<bool> IsInBattlePhase;

    void Awake()
    {
        _radioGameplay.SetGameManager(this);
    }

    void Start()
    {
        _roundManager = _radioGameplay.RoundManager;
        _roundManager.OnPhaseChanged += HandlePhaseChange;
    }

    void OnDisable()
    {
        _roundManager.OnPhaseChanged -= HandlePhaseChange;
    }

    public void InitStartingCredits(int startingCredits)
    {
        player1.Credits = startingCredits;
        player2.Credits = startingCredits;
        UpdateCreditsUI();
    }

    public void AddRoundCredits(int roundNumber)
    {
        if (_roundManager == null)
        {
            Debug.Log("no round manager");
            return;
        }

        if (roundNumber - 1 < _roundManager.creditsPerRound.Length)
        {
            player1.Credits += _roundManager.creditsPerRound[roundNumber - 1];
            player2.Credits += _roundManager.creditsPerRound[roundNumber - 1];
        }
        else
        {
            player1.Credits += _roundManager.creditsPerRound[_roundManager.creditsPerRound.Length - 1];
            player2.Credits += _roundManager.creditsPerRound[_roundManager.creditsPerRound.Length - 1];

        }
        UpdateCreditsUI();
    }

    public bool SpendCredits(int amount)
    {
        if (player1.Credits >= amount)
        {
            player1.Credits -= amount;
            UpdateCreditsUI();
            return true;
        }
        return false;
    }

    private void HandlePhaseChange(RoundPhase roundPhase)
    {
        IsInBattlePhase?.Invoke(roundPhase == RoundPhase.Combat);
    }

    public void UpdateCreditsUI()
    {
        _radioGameplay.RoundUIManager.UpdateCreditsUI(player1.Credits);
    }

}
