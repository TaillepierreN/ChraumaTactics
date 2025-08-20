using UnityEngine;
using System;

public enum RoundPhase { Preparation, Combat, PostCombat }

namespace CT.Gameplay
{
    public class RoundManager : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private Rd_Gameplay _radioGameplay;

        private GameManager _gameManager;


        [Header("Round Timers (seconds)")]
        [Min(1f)] public float prepTime = 15f;
        [Min(1f)] public float battleTime = 10f;
        [Min(1f)] public float postBattleTime = 5f;

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

        #region Unity Callbacks
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
                switch (CurrentPhase)
                {
                    case RoundPhase.Preparation:
                        BeginCombatPhase();
                        break;

                    case RoundPhase.Combat:
                        BeginPostCombatPhase();
                        break;

                    case RoundPhase.PostCombat:
                        BeginPreparationPhase();
                        break;

                    default:
                        Debug.Log("unknown phase");
                        break;
                }
            }
        }
        #endregion

        #region StartGame
        /// <summary>
        /// Start the game loop
        /// </summary>
        public void StartGame()
        {
            _radioGameplay.RoundUIManager.ShowRoundUI();
            BeginPreparationPhase();
            _gameStarted = true;
        }

        #endregion
        #region SwitchPhase

        /// <summary>
        /// skip to battle phase
        /// </summary>
        public void ForceEndPreparation()
        {
            if (CurrentPhase != RoundPhase.Preparation)
                return;
            BeginCombatPhase();
        }

        /// <summary>
        /// Start the preparation phase
        /// </summary>
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

            TriggerEvents();

            Debug.Log($"Preparation Started! Round {CurrentRound}");
        }

        /// <summary>
        /// Start the battle phase
        /// </summary>
        private void BeginCombatPhase()
        {
            CurrentPhase = RoundPhase.Combat;
            TimeRemaining = battleTime;

            TriggerEvents();

            Debug.Log($"Combat Started! Round {CurrentRound}");
        }

        /// <summary>
        /// Start the quick post battle phase
        /// </summary>
        private void BeginPostCombatPhase()
        {
            CurrentPhase = RoundPhase.PostCombat;
            TimeRemaining = postBattleTime;

            TriggerEvents();

            Debug.Log($"Post Combat Started! Round {CurrentRound}");
        }
        #endregion

        #region Helper

        /// <summary>
        /// Trigger event telling everyone when round/phase change is happening
        /// </summary>
        private void TriggerEvents()
        {
            OnRoundChanged?.Invoke(CurrentRound, CurrentPhase);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        #endregion
    }
}