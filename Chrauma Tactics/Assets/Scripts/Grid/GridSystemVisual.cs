using System.Collections.Generic;
using CT.Gameplay;
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
        [SerializeField] private Rd_Gameplay rd_Gameplay;

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
            rd_Gameplay.RoundManager.OnPhaseChanged += UpdateGridVisual;
            _gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    Transform gridSystemSingleTransform = Instantiate(_gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity, this.transform);
                    _gridSystemVisualSingleArray[x, z] = gridSystemSingleTransform.GetComponent<GridSystemVisualSingle>();
                    _gridSystemVisualSingleArray[x, z].Show();
                }
            }
            //HideAllGridPosition();
        }

        void OnDisable()
        {
            rd_Gameplay.RoundManager.OnPhaseChanged -= UpdateGridVisual;
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
        public void ShowAllGridPosition()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    _gridSystemVisualSingleArray[x, z].Show();
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

        private void UpdateGridVisual(RoundPhase phase)
        {
            HideAllGridPosition();
            switch (phase)
            {
                case RoundPhase.Preparation:
                    ShowAllGridPosition();
                    break;
                case RoundPhase.PostPreparation:
                    HideAllGridPosition();
                    break;
                default:
                    break;
            }
        }
        public void HideAllOverlays()
        {
            for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
            {
                for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
                {
                    _gridSystemVisualSingleArray[x, z].HideOverlay();
                }
            }
        }

        public void ShowOverlay(GridPosition gridPosition, Color color)
        {
            if (!LevelGrid.Instance.IsValidGridPosition(gridPosition))
                return;
            _gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].ShowOverlay(color);
        }

    }
}