using System.Collections.Generic;
using CT.Grid;
using UnityEngine;

namespace CT.Grid
{
    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance { get; private set;}

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

            _gridSystem = new GridSystem(gridX, gridZ, 2f);
            _gridSystem.CreateDebugObjects(_gridDebugObjectPrefab);
        }

        /*public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            _gridSystem.GetGridObject(gridPosition).AddUnit(unit);
        }
        public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
        {
            return _gridSystem.GetGridObject(gridPosition).GetUnitList();
        }
        public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
        {
            _gridSystem.GetGridObject(gridPosition).RemoveUnit(unit);
        }

        public void UnitMovedGridPosition(GridPosition  fromGridPosition, GridPosition toGridPosition, Unit unit)
        {
            RemoveUnitAtGridPosition(fromGridPosition, unit);
            AddUnitAtGridPosition(toGridPosition, unit);
        }*/

        public GridPosition GetGridPosition(Vector3 worldPosition) => _gridSystem.GetGridPosition(worldPosition);
        public bool IsValidGridPosition(GridPosition gridPosition) => _gridSystem.isValidGridPosition(gridPosition);
        public Vector3 GetWorldPosition(GridPosition gridPosition) => _gridSystem.GetWorldPosition(gridPosition);
        public int GetWidth() => _gridSystem.GetWidth();
        public int GetHeight() => _gridSystem.GetHeight();

        /*public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
        {
            GridObject gridObject = _gridSystem.GetGridObject(gridPosition);
            return gridObject.HasAnyUnit();
        }*/
    }
}
