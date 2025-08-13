using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentUI : MonoBehaviour
{
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

    }
}
