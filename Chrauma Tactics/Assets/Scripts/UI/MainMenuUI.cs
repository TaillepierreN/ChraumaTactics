using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;
    public GameObject signUpPanel;
    public GameObject signInPanel;


    public void OnSignUp()
    {
        if (signUpPanel != null && mainMenuPanel != null)
        {
            signUpPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            Debug.Log("Sign Up opened");
        }
    }

    public void OnSignIn()
    {
        if (signInPanel != null && signUpPanel != null)
        {
            signInPanel.SetActive(true);
            signUpPanel.SetActive(false);
            Debug.Log("Sign In opened");
        }
    }

    public void CloseSignUp()
    {
        if (signUpPanel != null && mainMenuPanel != null)
        {
            signUpPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            Debug.Log("Sign Up closed");
        }
    }

    public void CloseSignIn()
    {
        if (signInPanel != null && mainMenuPanel != null)
        {
            signInPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            Debug.Log("Sign In closed");
        }
    }

    public void OnPlay()
    {
        SceneManager.LoadScene("GameMenu");
    }

    public void OnSettings()
    {
        if (settingsPanel != null && mainMenuPanel != null)
        {
            settingsPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            Debug.Log("Settings opened");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && mainMenuPanel != null)
        {
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            Debug.Log("Settings closed");
        }
    }

    public void OnOptions()
    {
        Debug.Log("Options");
    }

    public void OnCommunity()
    {
        // Application.OpenURL("onverra");
    }
}