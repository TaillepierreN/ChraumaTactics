using UnityEngine;
using System;
using Unity.Netcode;
using CT.Tools;
using System.Collections.Generic;

public enum RoundPhase { Preparation, PostPreparation, Combat, PostCombat }

namespace CT.Gameplay
{
	public class RoundManager : NetworkBehaviour
	{

		public bool DebugMode = false;

		[Header("References")]
		[SerializeField] private Rd_Gameplay _radioGameplay;

		private GameManager _gameManager;


		[Header("Round Timers (seconds)")]
		[Min(1f)] public float prepTime = 15f;
		[Min(1f)] public float postPrepTime = 5f;
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
		private bool _p1WantsEndPrep = false;
		private bool _p2WantsEndPrep = false;

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

			if (!NetX.IsAuthoritative)
				return;
			_gameManager = _radioGameplay.GameManager;
			_gameManager.InitStartingCredits(creditsPerRound.Length > 0 ? creditsPerRound[0] : 0);
		}

		void Update()
		{
			if (!_gameStarted)
				return;
			if (NetX.IsAuthoritative)
			{
				TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
				OnTimerTick?.Invoke(TimeRemaining);

				if (TimeRemaining <= 0f)
				{
					switch (CurrentPhase)
					{
						case RoundPhase.Preparation:
							BeginPostPreparationPhase();
							break;

						case RoundPhase.PostPreparation:
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
			else
			{
				if (TimeRemaining > 0f)
				{
					TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
					OnTimerTick?.Invoke(TimeRemaining);
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
			if (_gameStarted) return;
			if (!NetX.IsListening)
			{
				_radioGameplay?.RoundUIManager?.ShowRoundUI();
				BeginPreparationPhase();
				_gameStarted = true;
				return;
			}
			if (NetX.IsServer)
			{
				ShowRoundUIClientRpc();
				_radioGameplay?.RoundUIManager?.ShowRoundUI();
				BeginPreparationPhase();
				_gameStarted = true;
			}
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
			BeginPostPreparationPhase();
		}

		/// <summary>
		/// Skip for client
		/// </summary>
		[ServerRpc(RequireOwnership = false)]
		public void ForceEndPreparationServerRpc()
		{
			if (CurrentPhase == RoundPhase.Preparation)
				BeginPostPreparationPhase();
		}

		/// <summary>
		/// Start the preparation phase
		/// </summary>
		private void BeginPreparationPhase()
		{
			CurrentPhase = RoundPhase.Preparation;
			TimeRemaining = prepTime;

			_p1WantsEndPrep = false;
			_p2WantsEndPrep = false;

			if (!_isFirstPrep)
			{
				CurrentRound++;

				_gameManager.AddRoundCredits(CurrentRound);
			}
			else
				_isFirstPrep = false;

			TriggerEvents();

			if (DebugMode)
				Debug.Log($"Preparation Started! Round {CurrentRound}");
		}

		/// <summary>
		/// Start the post preparation phase
		/// </summary>
		private void BeginPostPreparationPhase()
		{
			CurrentPhase = RoundPhase.PostPreparation;
			TimeRemaining = postPrepTime;

			_radioGameplay?.RoundUIManager?.ShowWaitingEndPrep(false);

			if (NetX.IsListening && IsServer)
				HideWaitingEndPrepClientRpc();

			TriggerEvents();

			if (DebugMode)
				Debug.Log($"PostPreparation Started! Round {CurrentRound}");
		}

		/// <summary>
		/// Start the battle phase
		/// </summary>
		private void BeginCombatPhase()
		{
			CurrentPhase = RoundPhase.Combat;
			TimeRemaining = battleTime;
			_radioGameplay?.RoundUIManager?.ShowWaitingEndPrep(false);
			TriggerEvents();

			if (DebugMode)
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

			if (DebugMode)
				Debug.Log($"Post Combat Started! Round {CurrentRound}");
		}
		#endregion

		#region Helper

		/// <summary>
		/// Trigger event telling everyone when round/phase change is happening
		/// </summary>
		private void TriggerEvents()
		{
			if (!NetX.IsListening)
			{
				OnRoundChanged?.Invoke(CurrentRound, CurrentPhase);
				OnPhaseChanged?.Invoke(CurrentPhase);
				return;
			}

			if (!IsServer)
				return;

			OnRoundChanged?.Invoke(CurrentRound, CurrentPhase);
			OnPhaseChanged?.Invoke(CurrentPhase);

			if (NetX.NM && NetX.IsHost)
			{
				List<ulong> targets = new List<ulong>(NetX.NM.ConnectedClientsIds);
				targets.Remove(NetX.NM.LocalClientId);
				if (targets.Count > 0)
				{
					PhaseChangedClientRpc(CurrentPhase, CurrentRound, TimeRemaining,
					new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = targets } });
				}
			}
			else
			{
				PhaseChangedClientRpc(CurrentPhase, CurrentRound, TimeRemaining);
			}
		}

		[ClientRpc]
		private void PhaseChangedClientRpc(RoundPhase phase, int round, float timeRemaining,
										ClientRpcParams rpcParams = default)
		{
			CurrentPhase = phase;
			CurrentRound = round;
			TimeRemaining = timeRemaining;
			if (!_gameStarted)
				_gameStarted = true;
			OnRoundChanged?.Invoke(round, phase);
			OnPhaseChanged?.Invoke(phase);
		}

		[ClientRpc]
		private void ShowRoundUIClientRpc()
		{
			_radioGameplay?.RoundUIManager?.ShowRoundUI();
			_radioGameplay.CommanderSelectionmenu?.HideWaiting();
		}

		[ServerRpc(RequireOwnership = false)]
		public void EndPreparationVoteServerRpc(Team team, ServerRpcParams rpcParams = default)
		{
			if (CurrentPhase != RoundPhase.Preparation) return;

			if (team == Team.Player1)
				_p1WantsEndPrep = true;
			else
				_p2WantsEndPrep = true;

			if (_p1WantsEndPrep && _p2WantsEndPrep)
			{
				BeginPostPreparationPhase();
			}
			else
			{
				ClientRpcParams target = new ClientRpcParams
				{
					Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
				};
				ShowWaitingEndPrepClientRpc(target);
			}
		}

		[ClientRpc]
		private void ShowWaitingEndPrepClientRpc(ClientRpcParams rpcParams = default)
		{
			_radioGameplay?.RoundUIManager?.ShowWaitingEndPrep(true);
		}

		[ClientRpc]
		private void HideWaitingEndPrepClientRpc()
		{
			_radioGameplay?.RoundUIManager?.ShowWaitingEndPrep(false);
		}

		[ClientRpc]
		private void AnnounceRoundResultClientRpc(int winningPlayer, int round)
		{
			if (round != CurrentRound) return;

			_radioGameplay?.RoundUIManager?.RoundResult(winningPlayer);
		}
		public void Server_AnnounceRoundResult(int winningPlayer)
		{
			if (!IsServer) return;
			AnnounceRoundResultClientRpc(winningPlayer, CurrentRound);
		}

		[ClientRpc]
		public void AnnounceEndResultClientRpc(int winningPlayer)
		{
			_radioGameplay?.RoundUIManager?.ShowEndGameResult(winningPlayer);
		}
		public void Server_AnnonceEndResult(int winningPlayer)
		{
			Debug.Log("server will announce the winner");
			if (!IsServer) return;
			AnnounceEndResultClientRpc(winningPlayer);
		}

		public void SetEndPhase(int winningPlayer)
		{
			_gameStarted = false;
			_radioGameplay?.RoundUIManager.ShowEndGameResult(winningPlayer);
		}
        #endregion
	}
}