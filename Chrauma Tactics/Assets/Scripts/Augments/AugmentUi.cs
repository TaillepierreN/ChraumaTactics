using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentUI : MonoBehaviour
{
    [SerializeField] private Rd_Gameplay _radioGameplay;
    public Team team;
    public Image iconImage;
    public TMP_Text augmentNameText;
    public TMP_Text unitTypeText;
    public TMP_Text rarityText;
    public TMP_Text bonusText;
    public TMP_Text costText;
    public TMP_Text statTypeText;

    private Augment augmentData;

    public void SetAugment(Augment augment)
    {
        augmentData = augment;

        statTypeText.text = augment.statType.ToString();
        augmentNameText.text = augment.augmentName;
        unitTypeText.text = augment.targetType.ToString();
        iconImage.sprite = augment.icon;
        rarityText.text = augment.rarity.ToString();
        bonusText.text = augment.bonusValue.ToString("F0") + "%";
        int cost = AugmentCosts.GetCost(augment.rarity);
        costText.text = cost.ToString();

        switch (augment.rarity)
        {
            case AugmentRarity.Silver:
                iconImage.color = new Color(0.75f, 0.75f, 0.75f);
                rarityText.color = new Color(0.75f, 0.75f, 0.75f);

                break;
            case AugmentRarity.Gold:
                iconImage.color = new Color(1f, 0.84f, 0f);
                rarityText.color = new Color(1f, 0.84f, 0f);
                break;
            case AugmentRarity.Quantum:
                iconImage.color = new Color(0.5f, 0f, 1f);
                rarityText.color = new Color(0.5f, 0f, 1f);
                break;
            default:
                iconImage.color = Color.white;
                break;
        }

    }

    public void SelectAugment()
    {
        if (augmentData == null)
            return;

        if (team == Team.Player1)
        {
            if (_radioGameplay.GameManager.player1.Credits < AugmentCosts.GetCost(augmentData.rarity))
                return;
        }
        else
        {
            if (_radioGameplay.GameManager.player2.Credits < AugmentCosts.GetCost(augmentData.rarity))
                return;
        }
        _radioGameplay.BoostManager.RegisterAugmentToTeam(team, augmentData);
        _radioGameplay.RoundUIManager.OnSkipAugmentSelection();
    }
}
