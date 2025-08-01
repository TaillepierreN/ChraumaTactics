using UnityEngine;
using CT.Grid;

namespace CT.Gameplay
{
    public class UnitPlacer : MonoBehaviour
    {
        public static UnitPlacer Instance { get; private set; }

        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private Material ValidMat;
        [SerializeField] private Material InvalidMat;

        private GameObject ghostUnit;
        private Renderer[] ghostRenderers;
        private bool isPlacing = false;

        private void Awake()
        {
            if (Instance == null)
                Destroy(Instance);
            Instance = this;
        }

        private void Update()
        {
            if (!isPlacing || ghostUnit == null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 worldPos = hit.point;
                GridPosition gridPos = LevelGrid.Instance.GetGridPosition(worldPos);
                bool isValid = LevelGrid.Instance.IsValidGridPosition(gridPos);

                ghostUnit.transform.position = LevelGrid.Instance.GetWorldPosition(gridPos);
                SetGhostMaterial(isValid ? ValidMat : InvalidMat);

                if (isValid && Input.GetMouseButtonDown(0))
                    PlaceUnit(gridPos);
            }
        }
        public void StartPlacingUnit(GameObject prefab)
        {
            unitPrefab = prefab;
            ghostUnit = Instantiate(unitPrefab);
            Destroy(ghostUnit.GetComponent<Unit>());
            ghostRenderers = ghostUnit.GetComponentsInChildren<Renderer>();
            isPlacing = true;
        }
        private void SetGhostMaterial(Material mat)
        {
            foreach (Renderer rend in ghostRenderers)
                rend.material = mat;
        }
        private void PlaceUnit(GridPosition pos)
        {
            Instantiate(unitPrefab, LevelGrid.Instance.GetWorldPosition(pos), Quaternion.identity);
            Destroy(ghostUnit);
            ghostUnit = null;
            isPlacing = false;
        }
    }


}
