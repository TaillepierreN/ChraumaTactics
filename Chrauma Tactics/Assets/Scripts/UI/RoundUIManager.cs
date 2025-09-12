using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using CT.Gameplay;
using CT.Tools;
using CT.UI;

public class RoundUIManager : MonoBehaviour
{
	[Header("Texts")]
	public TextMeshProUGUI roundText;
	public TextMeshProUGUI prepTimerText;
	public TextMeshProUGUI battleTimerText;
	public TMP_Text postBattleText;
	public TMP_Text PlayerOneName;
	public TMP_Text PlayerTwoName;

	[Header("Panels and buttons")]
	[SerializeField] private GameObject _roundUI;
	[SerializeField] private GameObject _waitingEndPrepOverlay;
	public GameObject prepUI;
	public GameObject augmentSelectionUI;
	public GameObject battleUI;
	public GameObject postBattleUI;
	public GameObject EndBattleUI;
	public CanvasGroup resultGroup;
	public GameObject endRoundButton;
	public CanvasGroup betweenRoundsPanel;

	[Header("Player Stats")]
	[SerializeField] Slider HPSliderP1;
	[SerializeField] Text HPTextP1;
	[SerializeField] Slider HPSliderP2;
	[SerializeField] Text HPTextP2;
	private int P1maxHP;
	private int P2maxHP;
	[Header("UI Options")]
	[SerializeField] private Team creditsForTeam = Team.Player1;

	[Header("Refs")]
	[SerializeField] private Rd_Gameplay _radioGameplay;
	[SerializeField] private TMP_Text creditsText;
	[SerializeField] private TMP_Text _winLoseBattleText;

	private RoundManager _roundManager;
	private GameManager _gameManager;
	private GameStateNetwork _stateNet;
	private bool _boundToState;
	private bool _offlineBOund;

	private int _currentRound = 1;
	private Coroutine betweenRoundsRoutine;

	void Awake()
	{
		_radioGameplay.SetRoundUIManager(this);
	}

	void Start()
	{
		_roundManager = _radioGameplay.RoundManager;
		_stateNet = _radioGameplay.GameStateNetwork;
		_gameManager = _radioGameplay.GameManager;
		StartCoroutine(BindStateWhenReady());
		TryBindOfflineIfNeeded();

		if (NetX.IsListening && NetX.NM && !NetX.IsServer)
			creditsForTeam = Team.Player2;

		if (_roundManager != null)
		{
			_roundManager.OnPhaseChanged += HandlePhaseChanged;
			_roundManager.OnRoundChanged += HandleRoundChanged;
			_roundManager.OnTimerTick += HandleTimerTick;

			HandleRoundChanged(_roundManager.CurrentRound, _roundManager.CurrentPhase);
			HandlePhaseChanged(_roundManager.CurrentPhase);
			HandleTimerTick(_roundManager.TimeRemaining);
		}
	}

	void OnDestroy()
	{
		if (_roundManager != null)
		{
			_roundManager.OnPhaseChanged -= HandlePhaseChanged;
			_roundManager.OnRoundChanged -= HandleRoundChanged;
			_roundManager.OnTimerTick -= HandleTimerTick;
		}
		if (_stateNet != null && _boundToState)
		{
			_stateNet.P1Credits.OnValueChanged -= OnP1CreditsChanged;
			_stateNet.P2Credits.OnValueChanged -= OnP2CreditsChanged;
			_stateNet.P1HP.OnValueChanged -= OnP1HpChanged;
			_stateNet.P2HP.OnValueChanged -= OnP2HpChanged;
		}
		if (_gameManager != null && _offlineBOund)
		{
			_gameManager.P1CreditsChanged -= UpdateCreditsUI;
			_gameManager.P2CreditsChanged -= UpdateCreditsUI;
		}
	}

	public void ShowRoundUI()
	{
		_roundUI.SetActive(true);
	}

	public void OnEndRoundButton()
	{
		if (_roundManager == null) return;

		if (!NetX.IsListening)
		{
			_roundManager.ForceEndPreparation();
			return;
		}
		Team myTeam = (NetX.IsListening && NetX.NM && !NetX.IsServer) ? Team.Player2 : Team.Player1;
		_roundManager.EndPreparationVoteServerRpc(myTeam);
		if (endRoundButton) endRoundButton.SetActive(false);
	}

	public void OnSkipAugmentSelection()
	{
		augmentSelectionUI.SetActive(false);
	}

	private IEnumerator BindStateWhenReady()
	{
		/* wait for networked state; if it never comes, we stay offline-bound*/
		while (_stateNet == null || !_stateNet.IsSpawned)
		{
			_stateNet = _radioGameplay.GameStateNetwork;
			yield return null;
		}

		if (_boundToState) yield break;
		_boundToState = true;

		if (NetX.IsListening && !(NetX.NM && NetX.IsServer))
			_stateNet.RequestFullStateSyncServerRpc();

		_stateNet.P1Credits.OnValueChanged += OnP1CreditsChanged;
		_stateNet.P2Credits.OnValueChanged += OnP2CreditsChanged;
		_stateNet.P1HP.OnValueChanged += OnP1HpChanged;
		_stateNet.P2HP.OnValueChanged += OnP2HpChanged;

		UpdateCreditsUI(_stateNet.P1Credits.Value, 1);
		UpdateCreditsUI(_stateNet.P2Credits.Value, 2);
		UpdatePlayerHp(1, _stateNet.P1HP.Value);
		UpdatePlayerHp(2, _stateNet.P2HP.Value);
	}

	private void TryBindOfflineIfNeeded()
	{

		if (NetX.IsListening) return;
		if (_offlineBOund) return;
		if (_gameManager == null) return;

		_offlineBOund = true;

		_gameManager.P1CreditsChanged += UpdateCreditsUI;

		UpdateCreditsUI(_gameManager.player1.Credits, 1);
		UpdateCreditsUI(_gameManager.player2.Credits, 2);
		UpdatePlayerHp(1, _gameManager.player1.HP);
		UpdatePlayerHp(2, _gameManager.player2.HP);
	}

	private void OnP1CreditsChanged(int oldV, int newV) => UpdateCreditsUI(newV, 1);
	private void OnP2CreditsChanged(int oldV, int newV) => UpdateCreditsUI(newV, 2);
	private void OnP1HpChanged(int oldV, int newV) => UpdatePlayerHp(1, newV);
	private void OnP2HpChanged(int oldV, int newV) => UpdatePlayerHp(2, newV);

	private void HandlePhaseChanged(RoundPhase phase)
	{
		switch (phase)
		{
			case RoundPhase.Preparation:
				prepUI.SetActive(true);
				if (_currentRound % 2 == 0)
					augmentSelectionUI.SetActive(true);
				postBattleUI.SetActive(false);
				endRoundButton.SetActive(true);
				resultGroup.alpha = 0;
				break;

			case RoundPhase.PostPreparation:
				prepUI.SetActive(false);
				endRoundButton.SetActive(false);
				ShowWaitingEndPrep(false);
				break;

			case RoundPhase.Combat:
				battleUI.SetActive(true);
				break;

			case RoundPhase.PostCombat:
				battleUI.SetActive(false);
				postBattleUI.SetActive(true);
				StartCoroutine(ShowResult());
				break;
		}
	}

	private void HandleRoundChanged(int round, RoundPhase phase)
	{
		if (roundText != null)
			roundText.text = $"Round {round} - {phase}";
		_currentRound = round;
	}

	private void HandleTimerTick(float TimeRemaining)
	{
		int seconds = Mathf.CeilToInt(TimeRemaining);

		switch (_roundManager.CurrentPhase)
		{
			case RoundPhase.Preparation:
				if (prepTimerText)
				{
					prepTimerText.gameObject.SetActive(true);
					prepTimerText.text = seconds.ToString();
				}
				if (battleTimerText) battleTimerText.gameObject.SetActive(false);
				break;

			case RoundPhase.Combat:
				if (battleTimerText)
				{
					battleTimerText.gameObject.SetActive(true);
					battleTimerText.text = seconds.ToString();
				}
				if (prepTimerText) prepTimerText.gameObject.SetActive(false);
				break;

			case RoundPhase.PostPreparation:
			case RoundPhase.PostCombat:
				if (betweenRoundsRoutine != null)
					StopCoroutine(betweenRoundsRoutine);

				betweenRoundsRoutine = StartCoroutine(ShowBetweenRoundsPanel(1f));

				if (battleTimerText)
				{
					battleTimerText.gameObject.SetActive(true);
					battleTimerText.text = seconds.ToString();
				}
				if (prepTimerText) prepTimerText.gameObject.SetActive(false);
				break;
		}
	}

	private IEnumerator ShowBetweenRoundsPanel(float duration)
	{
		if (betweenRoundsPanel)
		{
			betweenRoundsPanel.alpha = 0;
			betweenRoundsPanel.gameObject.SetActive(true);
			betweenRoundsPanel.alpha = 1;
			yield return new WaitForSeconds(0.9f);
			betweenRoundsPanel.alpha = 0;
			betweenRoundsPanel.gameObject.SetActive(false);
		}
	}


	public void SetPlayerHp(int playerHp, Team team)
	{
		if (team == Team.Player1)
		{
			HPSliderP1.maxValue = playerHp;
			HPSliderP1.value = playerHp;
			HPTextP1.text = $"{playerHp}/{playerHp}";
			P1maxHP = playerHp;
		}
		else
		{
			HPSliderP2.maxValue = playerHp;
			HPSliderP2.value = playerHp;
			HPTextP2.text = $"{playerHp}/{playerHp}";
			P2maxHP = playerHp;
		}
	}

	public void UpdatePlayerHp(int player, int playerHp)
	{
		if (player == 1)
		{
			HPSliderP1.value = playerHp;
			HPTextP1.text = $"{playerHp}/{P1maxHP}";
		}
		else
		{
			HPSliderP2.value = playerHp;
			HPTextP2.text = $"{playerHp}/{P2maxHP}";
		}
	}

	public void UpdateCreditsUI(int playerCred, int player)
	{
		if (creditsText == null)
			return;
		int myIndex = (creditsForTeam == Team.Player1) ? 1 : 2;
		if (player == myIndex)
		{
			/*Debug.Log($"[UI] Credits updated for me (P{myIndex}): {playerCred}");*/
			creditsText.text = playerCred.ToString();
		}
	}

	public void UpdatePlayerOneName(string name)
	{
		if (name != null && PlayerOneName != null)
		{
			PlayerOneName.text = $"P1 {name}";
		}
	}

	public void UpdatePlayerTwoName(string name)
	{
		if (name != null && PlayerTwoName != null)
		{
			PlayerTwoName.text = $"{name} P2";
		}
	}

	public void RoundResult(int winningPlayer)
	{
		if (winningPlayer == 3)
		{
			postBattleText.text = "Draw";
			return;
		}
		//Debug.Log("Winning is " + winningPlayer);
		int myIndex = !NetX.IsListening ? 1 : (NetX.IsServer ? 1 : 2);
		//Debug.Log($"my index is {myIndex}");
		postBattleText.text = (winningPlayer == myIndex) ? "Round Won" : "Round Lost";
	}

	private IEnumerator ShowResult()
	{
		float start = resultGroup.alpha;
		float time = 0f;
		while (time < 2f)
		{
			time += Time.unscaledDeltaTime;
			resultGroup.alpha = Mathf.Lerp(start, 1, time / 2f);
			yield return null;
		}
		resultGroup.alpha = 1;
	}

	public void ShowWaitingEndPrep(bool show)
	{
		if (_waitingEndPrepOverlay)
		{
			_waitingEndPrepOverlay.SetActive(show);
		}
		if (show)
		{
			prepUI.SetActive(false);
			endRoundButton.SetActive(false);
		}
	}

	public void ShowEndGameResult(int winningPlayer)
	{
		EndBattleUI.SetActive(true);
		int ownIndex = NetX.IsListening ? (NetX.IsHost ? 1 : 2) : 1;
		Debug.Log($"Winner is set winning player is {winningPlayer} and you are {ownIndex}");
		if (ownIndex == winningPlayer)
			_winLoseBattleText.text = "Victory";
		else
			_winLoseBattleText.text = "Defeat";
	}

	public void QuitBattle()
	{
		SceneLoader.Instance.LeaveBattle();
		Time.timeScale = 1f;
	}

}
