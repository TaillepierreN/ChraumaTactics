using UnityEngine;
using CT.Grid;
using System.Linq;

namespace CT.Gameplay
{
    public class UnitPlacer : MonoBehaviour
    {
        public static UnitPlacer Instance { get; private set; }

        [SerializeField] private GameObject squadPrefab;
        [SerializeField] private GameObject ghostSquadPrefab;
        [SerializeField] private GameObject unitPrefab;
        private int numberOfUnits = 1;


        private GameObject ghostUnit;

        private bool isPlacing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!isPlacing || ghostUnit == null)
                return;

            GridSystemVisual.Instance.HideAllOverlays();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GridPosition gridPos = LevelGrid.Instance.GetGridPosition(hit.point);
                bool isValid = LevelGrid.Instance.IsValidGridPosition(gridPos);

                ghostUnit.transform.position = LevelGrid.Instance.GetWorldPosition(gridPos);
                GridSystemVisual.Instance.ShowOverlay(gridPos, isValid ? Color.green : Color.red);

                if (isValid && Input.GetMouseButtonDown(0))
                    PlaceUnit(gridPos);
            }
        }
        public void StartPlacingUnit(GameObject unitToPlace, int nbrOfUnits = 1)
        {
            unitPrefab = unitToPlace;
            numberOfUnits = nbrOfUnits;

            ghostUnit = Instantiate(ghostSquadPrefab, Vector3.zero, Quaternion.identity);


            for (int i = 0; i < numberOfUnits; i++)
            {
                Transform spawnPos = ghostUnit.transform.GetChild(i);
                Debug.Log("Spawning ghost unit at: " + spawnPos.position);
                GameObject ghostUnitObj = Instantiate(unitPrefab, spawnPos.position, Quaternion.identity, spawnPos);
                DestroyImmediate(ghostUnitObj.GetComponent<Unit>());
                DestroyImmediate(ghostUnitObj.GetComponent<UnityEngine.AI.NavMeshAgent>());
                DestroyImmediate(ghostUnitObj.GetComponent<Animator>());
                foreach (var col in ghostUnitObj.GetComponentsInChildren<Collider>())
                    DestroyImmediate(col);
            }

            isPlacing = true;
        }

        private void PlaceUnit(GridPosition pos)
        {
            if (!isPlacing)
                return;
            isPlacing = false;
            GameObject SquadObject = Instantiate(squadPrefab, LevelGrid.Instance.GetWorldPosition(pos), Quaternion.identity);
            Squad squad = SquadObject.GetComponent<Squad>();

            squad.nbrOfUnits = numberOfUnits;
            squad.unitPrefab = unitPrefab;
            squad.SpawnUnit();

            Destroy(ghostUnit);
            ghostUnit = null;
        }
    }


}
