using UnityEngine;
using System.Collections.Generic;

public class AugmentSelectionMenu : MonoBehaviour
{
    [Header("Augment Selection")]
    public Augment[] allAugments;
    public GameObject augmentUIPrefab;
    public Transform container;

    void Start()
    {
        AugmentRarity chosenRarity = GetRandomRarity();

        List<Augment> augmentsOfRarity = new List<Augment>();
        foreach (Augment aug in allAugments)
        {
            if (aug.rarity == chosenRarity)
                augmentsOfRarity.Add(aug);
        }

        Shuffle(augmentsOfRarity);

        int count = Mathf.Min(3, augmentsOfRarity.Count);
        for (int i = 0; i < count; i++)
        {
            GameObject ui = Instantiate(augmentUIPrefab, container);
            ui.GetComponent<AugmentUI>().SetAugment(augmentsOfRarity[i]);
        }
    }

    private AugmentRarity GetRandomRarity()
    {
        float roll = Random.value * 100f;
        if (roll < 40f)
            return AugmentRarity.Silver;
        else if (roll < 75f)
            return AugmentRarity.Gold; 
        else
            return AugmentRarity.Quantum; 
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[rnd];
            list[rnd] = list[i];
            list[i] = temp;
        }
    }
}
