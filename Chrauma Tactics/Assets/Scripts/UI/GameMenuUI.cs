using CT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameMenuUI : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject gameMenuPanel;
    public GameObject popupQuitPanel;
    public GameObject rankingPanel;


    static bool NetActive()
    {
        var nm = NetworkManager.Singleton;
        return nm && nm.IsListening && (nm.IsServer || nm.IsClient);
    }

    static bool IsServer()
    {
        var nm = NetworkManager.Singleton;
        return nm && nm.IsListening && nm.IsServer;
    }

    public void OnPlay() => LoadGameEntry("LobbyPreGame");
    public void OnSolo() => LoadGameEntry("SampleScene");

    void LoadGameEntry(string sceneName)
    {
        if (!NetActive())
        {
            SceneLoader.LoadOffline(sceneName);
            return;
        }

        if (IsServer())
        {
            SceneLoader.BeginNetwork(sceneName);
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("[Menu] Waiting for host to start…");
            //disable the button for clients maybe
            // GetComponentInChildren<Button>().interactable = false;
        }
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