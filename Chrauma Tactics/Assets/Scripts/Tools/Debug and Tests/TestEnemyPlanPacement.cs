using UnityEngine;
using System.Collections.Generic;
using CT.Gameplay;
using NaughtyAttributes;

namespace CT.Tools.DebugAndTests
{
    [System.Serializable]
    public class PlanEntry
    {
        public EnemyAIPlan Plan;
        public Team Team = Team.Player2;
        [Min(1)] public int MaxRounds = 15;
    }

    /// <summary>
    /// Debug-only spawner that reads EnemyAIPlan assets and spawns ALL SquadOrders
    /// for ALL rounds immediately (no phases, no credits).
    /// </summary>
    public class TestEnemyPlanPacement : MonoBehaviour
    {
        [Header("Required")]
        public GameObject SquadPrefab;

        [Header("Plans to spawn")]
        public List<PlanEntry> Plans = new();

        [Header("Options")]
        public bool ClearBeforeSpawn = true;
        public bool LogOverlaps = true;
        public bool LogOutOfBounds = true;

        [Header("Bounds (inclusive)")]
        public Vector2 XRange = new Vector2(0f, 200f);
        public Vector2 ZRange = new Vector2(145f, 245f);

        // tracking
        private readonly List<GameObject> _spawned = new();
        private readonly HashSet<Vector3> _occupied = new();

#if UNITY_EDITOR
        [Button("Spawn ALL Plans Now")]
#endif
        public void SpawnAll()
        {
            if (!SquadPrefab)
            {
                Debug.LogWarning("[EnemyPlanDebugSpawner] SquadPrefab is not assigned.");
                return;
            }

            if (ClearBeforeSpawn) ClearSpawned();

            _occupied.Clear();
            int totalOrders = 0;
            int totalSquads = 0;
            int overlaps = 0;
            int oob = 0;

            foreach (PlanEntry entry in Plans)
            {
                if (entry == null || entry.Plan == null) continue;

                int rounds = Mathf.Min(entry.Plan.Rounds?.Count ?? 0, Mathf.Max(1, entry.MaxRounds));
                for (int roundIdx = 1; roundIdx <= rounds; roundIdx++)
                {
                    RoundPlanning rp = entry.Plan.GetPlanningForRound(roundIdx);
                    if (rp?.Spawns == null || rp.Spawns.Count == 0) continue;

                    foreach (SquadOrder order in rp.Spawns)
                    {
                        totalOrders++;

                        if (order == null || order.UnitPrefab == null)
                        {
                            Debug.LogWarning($"[EnemyPlanDebugSpawner] Missing order/unit in round {roundIdx} of plan {entry.Plan.name}");
                            continue;
                        }

                        Vector3 pos = order.Position;
                        Quaternion rot = Quaternion.Euler(order.EulerRotation);

                        if (_occupied.Contains(pos))
                        {
                            overlaps++;
                            if (LogOverlaps)
                                Debug.LogWarning($"[EnemyPlanDebugSpawner] OVERLAP at {pos} — plan {entry.Plan.name}, round {roundIdx}, unit {order.UnitPrefab.name}");
                        }
                        else
                        {
                            _occupied.Add(pos);
                        }

                        if (LogOutOfBounds && (pos.x < XRange.x || pos.x > XRange.y || pos.z < ZRange.x || pos.z > ZRange.y))
                        {
                            oob++;
                            Debug.LogWarning($"[EnemyPlanDebugSpawner] OUT OF BOUNDS at {pos} — allowed X {XRange.x}-{XRange.y}, Z {ZRange.x}-{ZRange.y} | plan {entry.Plan.name}, round {roundIdx}");
                        }


                        GameObject go = Instantiate(SquadPrefab, pos, rot, transform);
                        go.name = $"[DBG] {entry.Plan.name} R{roundIdx} - {order.UnitPrefab.name} x{Mathf.Max(1, order.NbrOfUnits)}";
                        _spawned.Add(go);

                        Squad squad = go.GetComponent<Squad>();
                        if (!squad)
                        {
                            Debug.LogError("[EnemyPlanDebugSpawner] SquadPrefab has no Squad component!");
                            continue;
                        }

                        squad.team = entry.Team;
                        squad.nbrOfUnits = Mathf.Max(1, order.NbrOfUnits);
                        squad.unitPrefab = order.UnitPrefab;
                        squad.SpawnUnit();

                        totalSquads++;
                    }
                }
            }

            Debug.Log($"[EnemyPlanDebugSpawner] Done: orders read={totalOrders}, squads spawned={totalSquads}, overlaps={overlaps}, outOfBounds={oob}");
        }

#if UNITY_EDITOR
        [Button("Clear Spawned")]
#endif
        public void ClearSpawned()
        {
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                var go = _spawned[i];
                if (go) DestroyImmediate(go);
            }
            _spawned.Clear();
            _occupied.Clear();
            Debug.Log("[EnemyPlanDebugSpawner] Cleared.");
        }
    }
}