using System.Collections.Generic;
using CT.Grid;
using UnityEngine;

namespace CT.Grid
{
    public class LevelGrid : MonoBehaviour
    {
        [System.Serializable]
        public struct GridRect
        {
            public int minX, maxX, minZ, maxZ;
            public bool Contains(GridPosition gp) =>
                gp.x >= minX && gp.x <= maxX && gp.z >= minZ && gp.z <= maxZ;
        }
        [SerializeField] private GridRect _p1PlacementArea;
        [SerializeField] private GridRect _p2PlacementArea;
        public bool DebugMode = false;
        public static LevelGrid Instance { get; private set; }

        public int gridX = 10;
        public int gridZ = 10;
        [SerializeField] private Transform _gridDebugObjectPrefab;
        private GridSystem _gridSystem;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There is more than one LevelGrid in the scene " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _gridSystem = new GridSystem(gridX, gridZ, 10f);
            if (DebugMode)
                _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab, this.gameObject);
        }

        public void AddSquadAtGridPosition(GridPosition gridPosition, Squad squad)
        {
            _gridSystem.GetGridObject(gridPosition).AddSquad(squad);
        }
        public List<Squad> GetSquadListAtGridPosition(GridPosition gridPosition)
        {
            return _gridSystem.GetGridObject(gridPosition).GetSquadList();
        }
        public void RemoveSquadAtGridPosition(GridPosition gridPosition, Squad squad)
        {
            _gridSystem.GetGridObject(gridPosition).RemoveSquad(squad);
        }

        public void SquadMovedGridPosition(GridPosition fromGridPosition, GridPosition toGridPosition, Squad squad)
        {
            RemoveSquadAtGridPosition(fromGridPosition, squad);
            AddSquadAtGridPosition(toGridPosition, squad);
        }

        public GridPosition GetGridPosition(Vector3 worldPosition) => _gridSystem.GetGridPosition(worldPosition);
        public bool IsValidGridPosition(GridPosition gridPosition) => _gridSystem.isValidGridPosition(gridPosition);
        public Vector3 GetWorldPosition(GridPosition gridPosition) => _gridSystem.GetWorldPosition(gridPosition);
        public int GetWidth() => _gridSystem.GetWidth();
        public int GetHeight() => _gridSystem.GetHeight();

        public bool HasAnySquadOnGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.HasAnySquad();
        }

        public bool IsInTeamArea(GridPosition gp, Team team)
        {
            return (team == Team.Player1 ? _p1PlacementArea : _p2PlacementArea).Contains(gp);
        }
        public IEnumerable<GridPosition> GetAllPositionsInTeamArea(Team team)
        {
            GridRect r = (team == Team.Player1) ? _p1PlacementArea : _p2PlacementArea;
            for (int x = r.minX; x <= r.maxX; x++)
                for (int z = r.minZ; z <= r.maxZ; z++)
                    yield return new GridPosition(x, z);
        }
    }

}
