using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuUI : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene("GameLobby");
    }

    public void OnSolo()
    {
        Debug.Log("Solo Mode");
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
        Debug.Log("Settings");
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
        Debug.Log("Rankings");
    }

    public void OnQuit()
    {
        Application.Quit();
        Debug.Log("Game exited");
    }
}