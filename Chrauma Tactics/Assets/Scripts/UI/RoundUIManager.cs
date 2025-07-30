using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundUIManager : MonoBehaviour
{
    public float prepTime = 15f;
    public float battleTime = 10f;
    private float currentTime;
    
    public TextMeshProUGUI prepTimerText;
    public TextMeshProUGUI battleTimerText;
    
    public GameObject endRoundButton;
    public GameObject prepUI;
    public GameObject battleUI;

    private enum Phase { Preparation, Combat }
    private Phase currentPhase;

    void Start()
    {
        StartPreparation();
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.Preparation:
                UpdatePrepTimerUI();
                if (currentTime <= 0f)
                    StartCombat();
                break;

            case Phase.Combat:
                UpdateBattleTimerUI();
                if (currentTime <= 0f)
                    StartPreparation();
                break;
        }
    }

    void UpdatePrepTimerUI()
    {
        prepTimerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    void UpdateBattleTimerUI()
    {
        battleTimerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    public void OnEndRoundButton()
    {
        StartCombat();
    }

    void StartPreparation()
    {
        currentPhase = Phase.Preparation;
        currentTime = prepTime;

        prepUI.SetActive(true);
        battleUI.SetActive(false);
        endRoundButton.SetActive(true);

        Debug.Log("Preparation Started!");
    }

    void StartCombat()
    {
        currentPhase = Phase.Combat;
        currentTime = battleTime;

        prepUI.SetActive(false);
        battleUI.SetActive(true);
        endRoundButton.SetActive(false);

        Debug.Log("Combat Started!");
    }
}
