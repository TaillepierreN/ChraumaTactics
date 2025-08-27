using UnityEngine;
using System;
using CT.Tools;
using Unity.Netcode;

namespace CT.Gameplay
{

    public class GameManager : NetworkBehaviour
    {
        public bool DebugMode = false;
        [Header("References")]
        [SerializeField] private Rd_Gameplay _radioGameplay;
        private RoundManager _roundManager;
        private GameStateNetwork _stateNet;



        [Header("Player stats")]
        public Player player1;
        public Player player2;
        [Min(1)] public int playerDamage = 500;

        [Header("Events")]
        public Action<RoundPhase> SetSquadPhase;
        public Action<int, int> P1CreditsChanged;
        public Action<int, int> P2CreditsChanged;
        public Action VoucherChanged;
        public void NotifyVoucherChanged() => VoucherChanged?.Invoke();

        #region  Unity callbacks
        void Awake()
        {
            _radioGameplay.SetGameManager(this);
        }

        void Start()
        {
            _roundManager = _radioGameplay.RoundManager;
            _stateNet = _radioGameplay.GameStateNetwork;

            if (_roundManager != null)
                _roundManager.OnPhaseChanged += HandlePhaseChange;
        }

        void OnDisable()
        {
            if (_roundManager != null)
                _roundManager.OnPhaseChanged -= HandlePhaseChange;
        }

        #endregion

        #region initialisation
        /// <summary>
        /// Set starting credits to player
        /// </summary>
        /// <param name="startingCredits"></param>
        public void InitStartingCredits(int startingCredits)
        {
            player1.Credits = startingCredits;
            player2.Credits = startingCredits;
            if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
            {
                _stateNet.P1Credits.Value = player1.Credits;
                _stateNet.P2Credits.Value = player2.Credits;
                _stateNet.P1HP.Value = player1.HP;
                _stateNet.P2HP.Value = player2.HP;
            }
            CreditsChanged();
        }

        /// <summary>
        /// Set hp corresponding to chosen commander
        /// TEMP: set both player with the same commander
        /// </summary>
        /// <param name="cmd"></param>
        public void SetChosenCommander(Commander cmd)
        {
            player1.HP = cmd.playerHealth;
            //temporary//
            player2.HP = cmd.playerHealth;
            _radioGameplay.RoundUIManager.SetPlayerHp(cmd.playerHealth);
            if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
            {
                _stateNet.P1HP.Value = player1.HP;
                _stateNet.P2HP.Value = player2.HP;
            }
        }

        #endregion

        #region Action

        /// <summary>
        /// give credits to player based on round number
        /// </summary>
        /// <param name="roundNumber"></param>
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
                CreditsChanged();
            }
            else
            {
                player1.Credits += _roundManager.creditsPerRound[_roundManager.creditsPerRound.Length - 1];
                player2.Credits += _roundManager.creditsPerRound[_roundManager.creditsPerRound.Length - 1];
                CreditsChanged();
            }
            if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
            {
                _stateNet.P1Credits.Value = player1.Credits;
                _stateNet.P2Credits.Value = player2.Credits;
            }
        }

        private void CreditsChanged()
        {
            P1CreditsChanged?.Invoke(player1.Credits, 1);
            P2CreditsChanged?.Invoke(player2.Credits, 2);
        }

        /// <summary>
        /// Remove credits based on unit/buff purchased
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool SpendCredits(int amount, Team team)
        {
            if (team == Team.Player1)
            {
                if (player1.Credits >= amount)
                {
                    player1.Credits -= amount;
                    P1CreditsChanged?.Invoke(player1.Credits, 1);
                    if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
                        _stateNet.P1Credits.Value = player1.Credits;
                    return true;
                }
            }
            else
            {
                if (player2.Credits >= amount)
                {
                    player2.Credits -= amount;
                    P2CreditsChanged?.Invoke(player2.Credits, 2);
                    if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
                        _stateNet.P2Credits.Value = player2.Credits;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if player can afford a purchase
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public bool CanAfford(int amount, Team team)
        {
            if (team == Team.Player1)
            {
                return player1.Credits >= amount;
            }
            else
            {
                return player2.Credits >= amount;
            }
        }

        /// <summary>
        /// tell all squad when phase change 
        /// in post preparation apply boosts
        /// in post combat, check who won
        /// </summary>
        /// <param name="roundPhase"></param>
        private void HandlePhaseChange(RoundPhase roundPhase)
        {
            SetSquadPhase?.Invoke(roundPhase);

            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

            if (roundPhase == RoundPhase.PostPreparation)
            {
                _radioGameplay.BoostManager.ApplyBoostsToArmy(player1.Army);
                _radioGameplay.BoostManager.ApplyBoostsToArmy(player2.Army);
            }

            if (roundPhase == RoundPhase.PostCombat)
                CheckWinRound();
        }

        /// <summary>
        /// Check who has the more surviving units to determine the winner of the round
        /// </summary>
        public void CheckWinRound()
        {
            int player1Survivors = 0;
            int player2Survivors = 0;

            foreach (Squad squad in player1.Army)
                player1Survivors += squad.nbrOfUnits - squad.nbrOfDeadUnit;
            foreach (Squad squad in player2.Army)
                player2Survivors += squad.nbrOfUnits - squad.nbrOfDeadUnit;
            if (DebugMode)
            {
                Debug.Log($"Player 1 has {player1Survivors} surviving units");
                Debug.Log($"Player 2 has {player2Survivors} surviving units");
            }
            if (player1Survivors > player2Survivors)
            {
                if (DebugMode)
                    Debug.Log("Player 1 win this round");
                _radioGameplay.RoundUIManager.RoundResult(1);
                DamagePlayer(2/*, player2Survivors*/);
            }
            else if (player1Survivors < player2Survivors)
            {
                if (DebugMode)
                    Debug.Log("Player 2 win this round");
                _radioGameplay.RoundUIManager.RoundResult(2);
                DamagePlayer(1/*, player1Survivors*/);
            }
            else
            {
                if (DebugMode)
                    Debug.Log("Draw");
                _radioGameplay.RoundUIManager.RoundResult(3);
                DamagePlayer(1/*, player2Survivors*/);
                DamagePlayer(2/*, player1Survivors*/);
            }
        }

        /// <summary>
        /// Deal damage to losing player
        /// </summary>
        /// <param name="player"></param>
        private void DamagePlayer(int player/*, int numberOfUnitAlive*/)
        {
            if (player == 1)
            {
                player1.HP -= playerDamage/* * numberofUnitAlive*/;
                if (player1.HP < 0)
                    Debug.Log($"Player 2 won the game");
                if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
                    _stateNet.P1HP.Value = player1.HP;
                else
                    _radioGameplay.RoundUIManager.UpdatePlayerHp(1, player1.HP);
            }
            else
            {
                player2.HP -= playerDamage/* * numberofUnitAlive*/;
                if (player2.HP < 0)
                    Debug.Log($"Player 1 won the game");
                if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
                    _stateNet.P2HP.Value = player2.HP;
                else
                    _radioGameplay.RoundUIManager.UpdatePlayerHp(2, player2.HP);
            }
        }
        #endregion

        #region Helper

        /// <summary>
        /// Add a spawned squad to the corresponding player army
        /// </summary>
        /// <param name="team"></param>
        /// <param name="squad"></param>
        public void AddToArmy(Team team, Squad squad)
        {
            if (team == Team.Player1)
                player1.Army.Add(squad);
            else
                player2.Army.Add(squad);
        }

        public void UpdateCreditsUI()
        {
            _radioGameplay.RoundUIManager.UpdateCreditsUI(player1.Credits, 1);
            _radioGameplay.RoundUIManager.UpdateCreditsUI(player2.Credits, 2);
        }

        public Player GetPlayerByTeam(Team team)
        {
            return team == Team.Player1 ? player1 : player2;
        }

        #endregion
    }
}