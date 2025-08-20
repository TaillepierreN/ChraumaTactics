using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace CT.Gameplay.Enemy
{
    public class EnemyAIRunner : MonoBehaviour
    {
        public enum PlanSelectMode { Single, Random }

        [Header("Data")]
        [Tooltip("List of plans for the AI enemy")]
        [SerializeField] private EnemyAIPlan[] planPool;

        [Header("Selection")]
        [SerializeField] private PlanSelectMode selectMode = PlanSelectMode.Random;
        [ShowIf("selectMode", PlanSelectMode.Single)]
        [Tooltip("if single mode is selected, use this plan index")]
        [SerializeField] private int singlePlanIndex = 0;

        [Header("Refs")]
        [SerializeField] private Rd_Gameplay _radioGameplay;
        [SerializeField] private Transform _spawnRoot;

        [Tooltip("Prefab of the squad to spawn")]
        [SerializeField] private GameObject _squadPrefab;

        [Header("Defaults")]
        [SerializeField] private Team defaultEnemyTeam = Team.Player2;

        private readonly HashSet<int> _spawnedRounds = new();
        private RoundManager _roundManager;
        private EnemyAIPlan _activePlan;
        private System.Random _rand;

        void Start()
        {
            if (_roundManager == null && _radioGameplay != null)
                _roundManager = _radioGameplay.RoundManager;

            _activePlan = ChoosePlan();
            if (_activePlan == null)
            {
                Debug.LogWarning("EnemyAIRunner: no plan was selected");
                return;
            }

            if (_squadPrefab == null)
            {
                Debug.LogWarning("EnemyAIRunner: Squad prefab is not set");
            }

            if (_roundManager != null)
                _roundManager.OnRoundChanged += HandleRoundChanged;
            else
                Debug.LogWarning("EnemyAIRunner: round manager is not set, cannot run ai plan");
        }

        private void OnDisable()
        {
            if (_roundManager != null)
                _roundManager.OnRoundChanged -= HandleRoundChanged;
        }

        /// <summary>
        /// Choose a plan from the pool based on the selection mode
        /// </summary>
        private EnemyAIPlan ChoosePlan()
        {
            _rand = new System.Random();

            if (planPool != null && planPool.Length > 0)
            {
                switch (selectMode)
                {
                    case PlanSelectMode.Random:
                        int r = _rand.Next(0, planPool.Length);
                        return planPool[r];

                    case PlanSelectMode.Single:
                        int index = Mathf.Clamp(singlePlanIndex, 0, planPool.Length - 1);
                        return planPool[index];

                    default:
                        Debug.LogWarning($"EnemyAIRunner: selection mode is unknown: {selectMode}");
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// Handle the round change event to spawn squads base on the current round index
        /// </summary>
        private void HandleRoundChanged(int roundIndex, RoundPhase phase)
        {
            if (phase != RoundPhase.Preparation)
                return;

            if (_spawnedRounds.Contains(roundIndex))
                return;

            SpawnForRound(roundIndex);
            _spawnedRounds.Add(roundIndex);
        }

        /// <summary>
        /// Spawn squads for the specified round index base on the active plan
        /// </summary>
        private void SpawnForRound(int roundIndex)
        {
            if (_activePlan == null || _squadPrefab == null)
                return;
            RoundPlanning round = _activePlan.GetPlanningForRound(roundIndex);
            if (round == null || round.Spawns == null || round.Spawns.Count == 0)
                return;

            foreach (SquadOrder squadOrder in round.Spawns)
                SpawnOneSquad(squadOrder);
        }

        /// <summary>
        /// Spawn a single squad based on the provided squad order
        /// </summary>
        private void SpawnOneSquad(SquadOrder order)
        {
            if (order == null || order.UnitPrefab == null)
                return;
            Vector3 worldPosition = order.Position;
            Quaternion rotation = Quaternion.Euler(order.EulerRotation);

            GameObject squadGO = Instantiate(_squadPrefab, worldPosition, rotation, _spawnRoot);

            Squad squad = squadGO.GetComponent<Squad>();
            if (squad == null)
            {
                Debug.LogWarning($"EnemyAIRunner: {squadGO.name} does not have a squad component");
                return;
            }

            squad.team = defaultEnemyTeam;

            squad.nbrOfUnits = Mathf.Max(1, order.NbrOfUnits);
            squad.unitPrefab = order.UnitPrefab;

            squad.SpawnUnit();
        }
    }
}