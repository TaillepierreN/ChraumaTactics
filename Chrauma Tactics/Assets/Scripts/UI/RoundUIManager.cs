using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundUIManager : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI prepTimerText;
    public TextMeshProUGUI battleTimerText;


    [Header("Panels and buttons")]
    [SerializeField] private GameObject _roundUI;

    public GameObject prepUI;
    public GameObject battleUI;
    public GameObject endRoundButton;

    [Header("Refs")]
    [SerializeField] private Rd_Gameplay _radioGameplay;
    [SerializeField] private TMP_Text creditsText;

    private RoundManager _roundManager;

    void Awake()
    {
        _radioGameplay.SetRoundUIManager(this);
    }

    void Start()
    {
        _roundManager = _radioGameplay.RoundManager;

        _roundManager.OnPhaseChanged += HandlePhaseChanged;
        _roundManager.OnRoundChanged += HandleRoundChanged;
        _roundManager.OnTimerTick += HandleTimerTick;

        HandleRoundChanged(_roundManager.CurrentRound, _roundManager.CurrentPhase);
        HandlePhaseChanged(_roundManager.CurrentPhase);
        HandleTimerTick(_roundManager.TimeRemaining);

    }

    void OnDestroy()
    {
        if (_roundManager == null)
            return;
        _roundManager.OnPhaseChanged -= HandlePhaseChanged;
        _roundManager.OnRoundChanged -= HandleRoundChanged;
        _roundManager.OnTimerTick -= HandleTimerTick;
    }

    public void ShowRoundUI()
    {
        _roundUI.SetActive(true);
    }
    public void OnEndRoundButton()
    {
        _roundManager.ForceEndPreparation();
    }

    private void HandlePhaseChanged(RoundPhase phase)
    {
        bool isPrep = phase == RoundPhase.Preparation;

        if (prepUI)
            prepUI.SetActive(isPrep);
        if (battleUI)
            battleUI.SetActive(!isPrep);

        if (endRoundButton)
            endRoundButton.SetActive(isPrep);
    }

    private void HandleRoundChanged(int round, RoundPhase phase)
    {
        if (roundText != null)
            roundText.text = $"Round {round} - {phase}";
    }

    private void HandleTimerTick(float TimeRemaining)
    {
        int seconds = Mathf.CeilToInt(TimeRemaining);

        if (_roundManager.CurrentPhase == RoundPhase.Preparation)
        {
            if (prepTimerText)
                prepTimerText.text = seconds.ToString();
        }
        else
        {
            if (battleTimerText)
                battleTimerText.text = seconds.ToString();
        }
    }

    public void UpdateCreditsUI(int playerCred)
    {
        if (creditsText != null)
            creditsText.text = playerCred.ToString();
    }
}
