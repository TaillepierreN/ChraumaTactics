using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace CT.Gameplay
{
    [CreateAssetMenu(fileName = "EnemyAIPlan", menuName = "Enemy/Plans", order = 0)]
    public class EnemyAIPlan : ScriptableObject
    {

        [Tooltip("Round 1 = index 0")]
        public List<RoundPlanning> Rounds = new();

        /// <summary>
        /// Get the planning for a specific round index.
        /// </summary>
        public RoundPlanning GetPlanningForRound(int roundIndex)
        {
            if (Rounds == null || Rounds.Count == 0 || roundIndex <= 0)
                return null;

            int index = roundIndex - 1;

            /*avoid error if not enough round planned*/
            if (index < 0 || index >= Rounds.Count)
                return null;

            return Rounds[index];
        }
    }

    [Serializable]
    public class RoundPlanning
    {
        public List<SquadOrder> Spawns = new();
    }

    [Serializable]
    public class SquadOrder
    {
        [Header("Squad Settings")]
        [Min(1)] public int NbrOfUnits = 1;

        [Tooltip("Prefab of the unit to spawn")]
        public GameObject UnitPrefab;

        [Header("Positioning")]
        [InfoBox("use [0 - 200], 0, [145 - 245]")]
        public Vector3 Position;
        public Vector3 EulerRotation = new Vector3(0, 180, 0);
    }
}
