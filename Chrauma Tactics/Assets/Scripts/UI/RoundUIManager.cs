using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using CT.Gameplay;

public class RoundUIManager : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI prepTimerText;
    public TextMeshProUGUI battleTimerText;
    public TMP_Text postBattleText;


    [Header("Panels and buttons")]
    [SerializeField] private GameObject _roundUI;

    public GameObject prepUI;
    public GameObject battleUI;
    public GameObject postBattleUI;
    public CanvasGroup resultGroup;
    public GameObject endRoundButton;

    [Header("Player Stats")]
    [SerializeField] Slider HPSliderP1;
    [SerializeField] Text HPTextP1;
    [SerializeField] Slider HPSliderP2;
    [SerializeField] Text HPTextP2;
    private int P1maxHP;
    private int P2maxHP;



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

    /// <summary>
    /// Check the phase to display the right UI
    /// </summary>
    /// <param name="phase"></param>
    private void HandlePhaseChanged(RoundPhase phase)
    {
        switch (phase)
        {
            case RoundPhase.Preparation:
                prepUI.SetActive(true);
                postBattleUI.SetActive(false);
                endRoundButton.SetActive(true);
                resultGroup.alpha = 0;
                break;

            case RoundPhase.Combat:
                prepUI.SetActive(false);
                battleUI.SetActive(true);
                endRoundButton.SetActive(false);
                break;

            case RoundPhase.PostCombat:
                battleUI.SetActive(false);
                postBattleUI.SetActive(true);
                StartCoroutine(ShowResult());
                break;
            default:
                break;
        }

    }

    private void HandleRoundChanged(int round, RoundPhase phase)
    {
        if (roundText != null)
            roundText.text = $"Round {round} - {phase}";
    }

    /// <summary>
    /// handle the timer display
    /// </summary>
    /// <param name="TimeRemaining"></param>
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

    /// <summary>
    /// set the player HP based on commander choice
    /// TEMP: both player use the same commander
    /// </summary>
    /// <param name="playerHp"></param>
    public void SetPlayerHp(int playerHp)
    {
        HPSliderP1.maxValue = playerHp;
        HPSliderP1.value = playerHp;
        //for now p2 has same hp as p1
        HPSliderP2.maxValue = playerHp;
        HPSliderP2.value = playerHp;
        HPTextP1.text = $"{playerHp}/{playerHp}";
        HPTextP2.text = $"{playerHp}/{playerHp}";
        P1maxHP = P2maxHP = playerHp;
    }

    /// <summary>
    /// Update the hp slider of players
    /// </summary>
    /// <param name="player"></param>
    /// <param name="playerHp"></param>
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

    /// <summary>
    /// Update the credits of player(one for now)
    /// </summary>
    /// <param name="playerCred"></param>
    public void UpdateCreditsUI(int playerCred)
    {
        if (creditsText != null)
            creditsText.text = playerCred.ToString();
    }

    /// <summary>
    /// set text of round winner
    /// </summary>
    /// <param name="winningPlayer"></param>
    public void RoundResult(int winningPlayer)
    {
        if (winningPlayer == 1)
            postBattleText.text = "Round Won";
        else if (winningPlayer == 2)
            postBattleText.text = "Round Lost";
        else
            postBattleText.text = "Draw";
    }

    /// <summary>
    /// fade in the result panel
    /// </summary>
    /// <returns></returns>
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
}
