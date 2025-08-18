using UnityEngine;
using TMPro;
using CT.Gameplay;
using System;

public enum RoundPhase { Preparation, Combat }
public class RoundManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Rd_Gameplay _radioGameplay;

    private GameManager _gameManager;


    [Header("Round Timers (seconds)")]
    [Min(1f)] public float prepTime = 15f;
    [Min(1f)] public float battleTime = 10f;

    [Header("Round Data")]
    public int[] creditsPerRound =
    {
        250, 300, 400, 450, 500, 600, 700, 850, 950,
        1000, 1100, 1200, 1350, 1500, 1600, 1700, 1800
    };

    public int CurrentRound { get; private set; } = 1;
    public RoundPhase CurrentPhase { get; private set; } = RoundPhase.Preparation;
    public float TimeRemaining { get; private set; }

    private bool _isFirstPrep = true;
    private bool _gameStarted = false;

    public event Action<RoundPhase> OnPhaseChanged;
    public event Action<int, RoundPhase> OnRoundChanged;
    public event Action<float> OnTimerTick;

    void Awake()
    {
        _radioGameplay.SetRoundManager(this);
    }

    void Start()
    {
        _gameManager = _radioGameplay.GameManager;

        _gameManager.InitStartingCredits(creditsPerRound.Length > 0 ? creditsPerRound[0] : 0);
    }

    void Update()
    {
        if (!_gameStarted)
            return;
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining < 0f)
            TimeRemaining = 0f;

        OnTimerTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            if (CurrentPhase == RoundPhase.Preparation)
                BeginCombatPhase();
            else
                BeginPreparationPhase();
        }
    }

    public void StartGame()
    {
        _radioGameplay.RoundUIManager.ShowRoundUI();
        BeginPreparationPhase();
        _gameStarted = true;
    }

    public void ForceEndPreparation()
    {
        if (CurrentPhase != RoundPhase.Preparation)
            return;
        BeginCombatPhase();
    }

    private void BeginPreparationPhase()
    {
        CurrentPhase = RoundPhase.Preparation;
        TimeRemaining = prepTime;

        if (!_isFirstPrep)
        {
            CurrentRound++;

            _gameManager.AddRoundCredits(CurrentRound);
        }
        else
            _isFirstPrep = false;

        OnRoundChanged?.Invoke(CurrentRound, CurrentPhase);
        OnPhaseChanged?.Invoke(CurrentPhase);

        Debug.Log($"Preparation Started! Round {CurrentRound}");
    }

    private void BeginCombatPhase()
    {
        CurrentPhase = RoundPhase.Combat;
        TimeRemaining = battleTime;

        OnRoundChanged?.Invoke(CurrentRound, CurrentPhase);
        OnPhaseChanged?.Invoke(CurrentPhase);

        Debug.Log($"Combat Started! Round {CurrentRound}");
    }






}
