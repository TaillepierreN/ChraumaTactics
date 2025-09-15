using UnityEngine;

namespace CT.Tools
{

    public class PoolRegisterer : MonoBehaviour
    {
        [SerializeField] private Rd_Gameplay _radioGameplay;

        void Awake()
        {
            _radioGameplay.SetPoolContainer(this.transform);
        }
    }
}
