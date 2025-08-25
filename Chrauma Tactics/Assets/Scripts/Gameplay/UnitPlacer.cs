using UnityEngine;
using CT.Grid;
using System.Linq;
using System.Collections.Generic;

namespace CT.Gameplay
{
    public class UnitPlacer : MonoBehaviour
    {
        public static UnitPlacer Instance { get; private set; }

        [SerializeField] private GameObject squadPrefab;
        [SerializeField] private GameObject ghostSquadPrefab;
        [SerializeField] private Rd_Gameplay _radioGameplay;
        private GameObject unitPrefab;
        private int numberOfUnits = 1;
        private Team placingTeam;
        private bool usingVoucher = false;
        private GameObject voucherUnitPrefab = null;


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

        private void Start()
        {
            _radioGameplay.RoundManager.OnPhaseChanged += HandleChangePhase;
        }

        private void OnDestroy()
        {
            _radioGameplay.RoundManager.OnPhaseChanged -= HandleChangePhase;
        }

        private void HandleChangePhase(RoundPhase phase)
        {
            if (phase == RoundPhase.PostPreparation)
                ClearGhostUnit();
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
                bool isValid = LevelGrid.Instance.IsValidGridPosition(gridPos) &&
                    !LevelGrid.Instance.HasAnySquadOnGridPosition(gridPos);

                ghostUnit.transform.position = LevelGrid.Instance.GetWorldPosition(gridPos);
                GridSystemVisual.Instance.ShowOverlay(gridPos, isValid ? Color.green : Color.red);

                if (isValid && Input.GetMouseButtonDown(0))
                    PlaceUnit(gridPos);
            }
        }

        public void StartPlacingUnit(GameObject unitToPlace, int nbrOfUnits = 1, Team team = Team.Player1)
        {
            placingTeam = team;
            usingVoucher = false;
            voucherUnitPrefab = null;
            BeginPlacing(unitToPlace, nbrOfUnits);
        }

        public void StartPlacingFreeUnit(GameObject unitToPlace, int nbrOfUnits = 1, Team team = Team.Player1)
        {
            placingTeam = team;
            usingVoucher = true;
            voucherUnitPrefab = unitToPlace;
            BeginPlacing(unitToPlace, nbrOfUnits);
        }

        private void BeginPlacing(GameObject unitToPlace, int nbrOfUnits)
        {
            unitPrefab = unitToPlace;
            numberOfUnits = nbrOfUnits;

            ghostUnit = Instantiate(ghostSquadPrefab, Vector3.zero, Quaternion.identity);

            List<Vector3> formation = SquadFormationPresets.GetFormation(nbrOfUnits);

            if (formation == null || formation.Count < nbrOfUnits)
            {
                Debug.LogError($"Pas de formation disponible pour {nbrOfUnits} unités.");
                return;
            }

            for (int i = 0; i < numberOfUnits; i++)
            {
                Vector3 spawnPos = formation[i];
                GameObject ghostUnitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity, ghostUnit.transform);

                DestroyImmediate(ghostUnitObj.GetComponent<Unit>());
                DestroyImmediate(ghostUnitObj.GetComponent<UnityEngine.AI.NavMeshAgent>());
                DestroyImmediate(ghostUnitObj.GetComponent<Animator>());
                foreach (var col in ghostUnitObj.GetComponentsInChildren<Collider>())
                    DestroyImmediate(col);
            }

            isPlacing = true;
            return;
        }

        private void PlaceUnit(GridPosition pos)
        {
            if (!isPlacing)
                return;
            isPlacing = false;
            GameObject SquadObject = Instantiate(squadPrefab, LevelGrid.Instance.GetWorldPosition(pos), Quaternion.identity);
            Squad squad = SquadObject.GetComponent<Squad>();

            squad.team = placingTeam;
            squad.nbrOfUnits = numberOfUnits;
            squad.unitPrefab = unitPrefab;
            squad.SpawnUnit();

            LevelGrid.Instance.AddSquadAtGridPosition(pos, squad);

            if (usingVoucher && voucherUnitPrefab != null)
            {
                Player player = _radioGameplay.GameManager.GetPlayerByTeam(placingTeam);
                if (player != null)
                    player.ConsumeFreeSquadVoucher(voucherUnitPrefab);
            }
            usingVoucher = false;
            voucherUnitPrefab = null;

            if (placingTeam == Team.Player1)
                _radioGameplay.GameManager.P1CreditsChanged?.Invoke(_radioGameplay.GameManager.player1.Credits);
            else
                _radioGameplay.GameManager.P2CreditsChanged?.Invoke(_radioGameplay.GameManager.player2.Credits);


            ClearGhostUnit();
        }

        private void ClearGhostUnit()
        {
            isPlacing = false;
            GridSystemVisual.Instance.HideAllOverlays();
            Destroy(ghostUnit);
            ghostUnit = null;
        }
    }


}
