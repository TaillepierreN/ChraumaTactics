using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CT.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool isPauseOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu()
    {
        isPauseOpen = !isPauseOpen;
        pauseMenu.SetActive(isPauseOpen);
    }

    public void ClosePauseMenu()
    {
        isPauseOpen = false;
        pauseMenu.SetActive(false);
    }

    public void BackToMenu()
    {
        SceneLoader.Instance.LeaveBattle();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
