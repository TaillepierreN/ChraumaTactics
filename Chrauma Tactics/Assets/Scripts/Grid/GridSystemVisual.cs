using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CT.Grid
{
    /// <summary>
    /// Represents the visual representation of the grid system.
    /// </summary>
    public class GridSystemVisual : MonoBehaviour
    {
        public static GridSystemVisual Instance { get; private set; }

        [SerializeField] private Transform _gridSystemVisualSinglePrefab;

        private GridSystemVisualSingle[,] _gridSystemVisualSingleArray;


        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("There is more than one GridSystemVisual in the scene " + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        void Start()
        {
            _gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    Transform gridSystemSingleTransform = Instantiate(_gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                    _gridSystemVisualSingleArray[x, z] = gridSystemSingleTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
            HideAllGridPosition();
        }

        public void HideAllGridPosition()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    _gridSystemVisualSingleArray[x, z].Hide();
                }
            }
        }


        public void ShowGridPositionList(List<GridPosition> gridPositionList)
        {
            foreach (GridPosition gridPosition in gridPositionList)
            {
                _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show();
            }
        }

        private void UpdateGridVisual()
        {
            //HideAllGridPosition();
            //Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        
        // TEMPORARY DEBUG GRID VISUAL 
        ShowGridPositionList(new List<GridPosition>
        {
        new GridPosition(0, 0),
        new GridPosition(0, 10),
        new GridPosition(20, 10),
        new GridPosition(20, 10)
        });
            //ShowGridPositionList(selectedUnit.GetMoveAction().GetValidGridPositionList());
        }

        void Update()
        {
            UpdateGridVisual();
        }
    }
}