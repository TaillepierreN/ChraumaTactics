using UnityEngine;

namespace CT.Tools
{

    public static class TeamSquadPool
    {
        private static Transform _root;
        private static Transform _p1;
        private static Transform _p2;

        public static Transform Get(Team team)
        {
            Ensure();
            return team == Team.Player1 ? _p1 : _p2;
        }

        private static void Ensure()
        {
            if (_root && _p1 && _p2) return;

            _root = GameObject.Find("--- Armies ---")?.transform
                    ?? new GameObject("--- Armies ---").transform;

            _p1 = _root.Find("P1_Squads")
                  ?? new GameObject("P1_Squads").transform;
            _p1.SetParent(_root, true);

            _p2 = _root.Find("P2_Squads")
                  ?? new GameObject("P2_Squads").transform;
            _p2.SetParent(_root, true);
        }
    }
}
