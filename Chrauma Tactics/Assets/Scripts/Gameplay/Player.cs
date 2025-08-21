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
    }

}