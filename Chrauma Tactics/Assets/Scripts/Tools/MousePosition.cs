using CT.Grid;
using UnityEngine;

namespace CT.Tools
{
    public class MousePosition : MonoBehaviour
    {

        [SerializeField] private LayerMask floorLayerMask;
        private static MousePosition instance;
        private void Awake() {
            instance = this;
        }

        public static Vector3 GetPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, instance.floorLayerMask);
            return hit.point;
        }
    }
}
