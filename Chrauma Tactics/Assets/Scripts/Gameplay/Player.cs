using System.Collections.Generic;
using UnityEngine;

namespace CT.Gameplay
{
    [System.Serializable]
    public class Player
    {
        public Team Team;
        public int Credits;
        public int HP;
        public List<Squad> Army;
        public List<GameObject> FreeSquadVouchers = new List<GameObject>();

        public void GiveFreeSquadVoucher(GameObject unitPrefab)
        {
            if (unitPrefab == null) return;
            FreeSquadVouchers.Add(unitPrefab);
        }
        public void ConsumeFreeSquadVoucher(GameObject unitPrefab)
        {
            if (unitPrefab == null) return;
            FreeSquadVouchers.Remove(unitPrefab);
        }
    }

}