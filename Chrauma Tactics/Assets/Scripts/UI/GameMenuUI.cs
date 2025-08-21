using CT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuUI : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject gameMenuPanel;
    public GameObject popupQuitPanel;
    public GameObject rankingPanel;




    public void OnPlay()
    {
        SceneLoader.Load("LobbyPreGame");
    }

    public void OnSolo()
    {
        SceneLoader.Load("SampleScene");
    }

    public void OnAboutUs()
    {
        Debug.Log("About Us");
    }

    public void OnAccount()
    {
        Debug.Log("Account");
    }

    public void OnSettings()
    {
        if (settingsPanel != null && gameMenuPanel != null)
        {
            settingsPanel.SetActive(true);
            gameMenuPanel.SetActive(false);
            Debug.Log("Settings opened");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && gameMenuPanel != null)
        {
            settingsPanel.SetActive(false);
            gameMenuPanel.SetActive(true);
            Debug.Log("Settings closed");
        }
    }

    public void OnCommunity()
    {
        // Application.OpenURL("onverra");
    }

    public void OnCollection()
    {
        Debug.Log("Collection");
    }

    public void OnSkills()
    {
        Debug.Log("Skills");
    }

    public void OnTutorials()
    {
        Debug.Log("Tutorials");
    }

    public void OnRankings()
    {
        if (rankingPanel != null && gameMenuPanel != null)
        {
            rankingPanel.SetActive(true);
            gameMenuPanel.SetActive(false);
            Debug.Log("Rankings opened");
        }
    }

    public void CloseRankings()
    {
        if (rankingPanel != null && gameMenuPanel != null)
        {
            rankingPanel.SetActive(false);
            gameMenuPanel.SetActive(true);
            Debug.Log("Rankings closed");
        }
    }

    public void OnQuit()
    {
        Application.Quit();
        Debug.Log("Game exited");
    }

    public void OnPopupQuit()
    {
        if (popupQuitPanel != null)
        {
            popupQuitPanel.SetActive(true);
            Debug.Log("Quit confirmation popup opened");
        }
    }
    public void ClosePopupQuit()
    {
        if (popupQuitPanel != null)
        {
            popupQuitPanel.SetActive(false);
            Debug.Log("Quit confirmation popup closed");
        }
    }
    
}