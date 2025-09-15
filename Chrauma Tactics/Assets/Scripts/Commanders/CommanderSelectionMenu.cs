using System.Collections.Generic;
using UnityEngine;

public class CommanderSelectionMenu : MonoBehaviour
{
    [SerializeField] private Rd_Gameplay _radioGameplay;
    [Header("Commander Selection")]
    public Commander[] commanders;
    public GameObject commanderUIPrefab;
    public Transform container;
    private List<CommanderUI> _listOfComUI = new();
    [SerializeField] private GameObject _commanderSelectPanel;

    [Header("Waiting Overlay")]
    [SerializeField] private GameObject _waitingOverlay;


    void Awake()
    {
        _radioGameplay.SetCommanderSelectionMenu(this);
        if (_waitingOverlay)
            _waitingOverlay.SetActive(false);
    }
    void Start()
    {
        Shuffle(commanders);

        int count = Mathf.Min(3, commanders.Length);
        for (int i = 0; i < count; i++)
        {
            GameObject ui = Instantiate(commanderUIPrefab, container);
            CommanderUI comUI = ui.GetComponent<CommanderUI>();
            comUI.SetCommander(commanders[i]);
            _listOfComUI.Add(comUI);
            comUI.CommanderChosen += GameStart;
        }
    }

    void OnDisable()
    {
        if (_listOfComUI.Count != 0)
        {
            foreach (CommanderUI comUI in _listOfComUI)
            {
                if (comUI != null)
                    comUI.CommanderChosen -= GameStart;
            }
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

    private void GameStart()
    {
        _commanderSelectPanel.SetActive(false);
    }

    public void ShowWaiting()
    {
        if (_commanderSelectPanel) _commanderSelectPanel.SetActive(false);
        if (_waitingOverlay) _waitingOverlay.SetActive(true);
    }

    public void HideWaiting()
    {
        if (_waitingOverlay) _waitingOverlay.SetActive(false);
    }
}
