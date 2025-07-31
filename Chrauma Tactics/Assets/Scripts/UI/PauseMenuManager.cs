using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject settingsMenu;
    private bool isSettingsOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsMenu();
        }
    }

    void ToggleSettingsMenu()
    {
        isSettingsOpen = !isSettingsOpen;
        settingsMenu.SetActive(isSettingsOpen);
    }

    public void CloseSettingsMenu()
    {
        isSettingsOpen = false;
        settingsMenu.SetActive(false);
    }
}
