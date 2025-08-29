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

        public void RequestCommanderSelection(Commander cmd, Team team)
        {
            int hp = cmd.playerHealth;

            if (NetX.IsListening && !IsServer)
            {
                SelectCommanderServerRpc(hp);
                return;
            }
            ApplyCommanderHPForLocalTeam(team, hp);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectCommanderServerRpc(int hp, ServerRpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            Team team = (sender == NetworkManager.ServerClientId) ? Team.Player1 : Team.Player2;
            ApplyCommanderHPForLocalTeam(team, hp);
        }

        /// <summary>
        /// Set hp corresponding to chosen commander
        /// use the same for both player in offline
        /// </summary>
        /// <param name="cmd"></param>
        private void ApplyCommanderHPForLocalTeam(Team team, int hp)
        {

            if (!NetX.IsListening)
            {
                player1.HP = hp;
                player2.HP = hp;

                _radioGameplay.RoundUIManager.SetPlayerHp(hp, Team.Player1);
                _radioGameplay.RoundUIManager.SetPlayerHp(hp, Team.Player2);
                return;
            }

            if (team == Team.Player1) player1.HP = hp; else player2.HP = hp;

            if (NetX.IsListening && IsServer && _stateNet != null && _stateNet.IsSpawned)
            {
                _stateNet.P1HP.Value = player1.HP;
                _stateNet.P2HP.Value = player2.HP;
                InitHpBarClientRpc((int)team, hp);
            }

            if (!NetX.IsListening)
                _radioGameplay.RoundUIManager.SetPlayerHp(hp, team);
        }

        [ServerRpc(RequireOwnership = false)]
        public void GrantVoucherServerRpc(int unitIndex, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (PlacementNetwork.Instance == null) return;

            ulong sender = rpcParams.Receive.SenderClientId;
            Team team = (sender == NetworkManager.ServerClientId) ? Team.Player1 : Team.Player2;

            GameObject prefab = PlacementNetwork.Instance.GetUnitByIndex(unitIndex);
            if (prefab == null) return;

            Player player = GetPlayerByTeam(team);
            player?.GiveFreeSquadVoucher(prefab);

            MirrorVoucherAddClientRpc(unitIndex, (int)team, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { sender } }
            });

            NotifyVoucherChangedClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { sender } }
            });
        }

        [ClientRpc]
        private void MirrorVoucherAddClientRpc(int unitIndex, int teamInt, ClientRpcParams _ = default)
        {
            PlacementNetwork pn = PlacementNetwork.Instance;
            GameManager gm = this;
            if (pn == null || gm == null) return;

            GameObject prefab = pn.GetUnitByIndex(unitIndex);
            Team team = (Team)teamInt;
            Player player = gm.GetPlayerByTeam(team);
            player?.GiveFreeSquadVoucher(prefab);
        }

        [ClientRpc]
        private void NotifyVoucherChangedClientRpc(ClientRpcParams _ = default)
        {
            NotifyVoucherChanged();
        }

        [ClientRpc]
        private void InitHpBarClientRpc(int teamInt, int hp)
        {
            _radioGameplay.RoundUIManager.SetPlayerHp(hp, (Team)teamInt);
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

            if (NetX.IsListening && (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)) return;

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
            if (NetX.IsListening && !NetX.IsServer) return;

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
            int winner;
            if (player1Survivors > player2Survivors)
            {
                if (DebugMode)
                    Debug.Log("Player 1 win this round");
                winner = 1;
            }
            else if (player1Survivors < player2Survivors)
            {
                if (DebugMode)
                    Debug.Log("Player 2 win this round");
                winner = 2;
            }
            else
            {
                if (DebugMode)
                    Debug.Log("Draw");
                winner = 3;
            }
            if (!NetX.IsListening || NetX.IsServer)
            {
                switch (winner)
                {
                    case 1:
                        DamagePlayer(2/*, player2Survivors*/);
                        break;
                    case 2:
                        DamagePlayer(1/*, player1Survivors*/);
                        break;
                    default:
                        DamagePlayer(1/*, player2Survivors*/);
                        DamagePlayer(2/*, player1Survivors*/);
                        break;
                }
            }
            if (!NetX.IsListening)
                _radioGameplay.RoundUIManager.RoundResult(winner);
            else if (NetX.IsServer)
                _radioGameplay.RoundManager.Server_AnnounceRoundResult(winner);
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