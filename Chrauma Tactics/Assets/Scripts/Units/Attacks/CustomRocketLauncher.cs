using UnityEngine;

namespace CT.Units.Attacks
{
    public class CustomRocketLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject[] RocketLauncher;

        /// <summary>
        /// Hide gameobject to give the illusion of shooting
        /// </summary>
        /// <param name="slot">which rocket</param>
        public void ShootRocket(int slot) => RocketLauncher[slot].SetActive(false);
        /// <summary>
        /// Show gameobject to give the illusion of reloading
        /// </summary>
        /// <param name="slot">which rocket</param>
        public void ReloadRocker(int slot) => RocketLauncher[slot].SetActive(true);

    }
}