using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Round Data")]
    public int playerCredits = 0;
    public int[] creditsPerRound =
    {
        250, 300, 400, 450, 500, 600, 700, 850, 950,
        1000, 1100, 1200, 1350, 1500, 1600, 1700, 1800
    };

    [Header("UI")]
    public TMP_Text creditsText;

    void Start()
    {

        playerCredits = creditsPerRound[0];
        UpdateCreditsUI();
    }

    public void AddRoundCredits(int roundNumber)
    {
        if (roundNumber - 1 < creditsPerRound.Length)
            playerCredits += creditsPerRound[roundNumber - 1];
        else
            playerCredits += creditsPerRound[creditsPerRound.Length - 1];

        UpdateCreditsUI();
    }

    public bool SpendCredits(int amount)
    {
        if (playerCredits >= amount)
        {
            playerCredits -= amount;
            UpdateCreditsUI();
            return true;
        }
        return false;
    }

    private void UpdateCreditsUI()
    {
        if (creditsText != null)
            creditsText.text = playerCredits.ToString();
    }
}
