using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene("GameMenu"); 
    }

    public void OnLogin()
    {
        Debug.Log("Login");
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