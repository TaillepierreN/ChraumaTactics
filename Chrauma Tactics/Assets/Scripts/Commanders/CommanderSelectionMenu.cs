using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CommanderSelectionMenu : MonoBehaviour
{
    [Header("Commander Selection")]
    public Commander[] commanders;
    public GameObject commanderUIPrefab;
    public Transform container;

    void Start()
    {
        Shuffle(commanders);

        int count = Mathf.Min(3, commanders.Length);
        for (int i = 0; i < count; i++)
        {
            GameObject ui = Instantiate(commanderUIPrefab, container);
            ui.GetComponent<CommanderUI>().SetCommander(commanders[i]);
        }
    }

    void Shuffle(Commander[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(i, array.Length);
            Commander temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }
    }
}
