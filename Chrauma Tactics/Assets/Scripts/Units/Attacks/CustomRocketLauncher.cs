using UnityEngine;

public class CustomRocketLauncher : MonoBehaviour
{
    [SerializeField] private GameObject[] RocketLauncher;

    public void ShootRocket(int slot)
    {
        Debug.Log($"shooting {slot}");
        RocketLauncher[slot].SetActive(false);
    }

    public void ReloadRocker(int slot)
    {
        RocketLauncher[slot].SetActive(true);
    }

}
