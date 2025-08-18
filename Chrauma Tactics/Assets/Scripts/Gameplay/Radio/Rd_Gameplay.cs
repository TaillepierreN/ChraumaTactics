using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Gameplay", menuName = "Radio/Gameplay")]
public class Rd_Gameplay : ScriptableObject
{
    private GameManager _gameManager;
    private RoundManager _roundManager;
    private RoundUIManager _roundUIManager;
    private TMP_Text _creditsText;


    public GameManager GameManager => _gameManager;
    public RoundManager RoundManager => _roundManager;
    public RoundUIManager RoundUIManager => _roundUIManager;
    public TMP_Text CreditsText => _creditsText;

    public void SetGameManager(GameManager gm)
    {
        _gameManager = gm;
    }

    public void SetRoundManager(RoundManager rm)
    {
        _roundManager = rm;
    }

    public void SetRoundUIManager(RoundUIManager ruim)
    {
        _roundUIManager = ruim;
    }

    public void SetCreditsText(TMP_Text ct)
    {
        _creditsText = ct;
    }
}
