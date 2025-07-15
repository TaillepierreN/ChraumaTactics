using UnityEngine;
using System;
using System.Numerics;

namespace CT.Grid
{
	public class GridSystem
	{
		private int _width;
		private int _height;
		private float _cellSize;
		private GridObject[,] _gridObjectArray;

		/// <summary>
		/// Initializes a new instance of the <see cref="GridSystem"/> class with the specified width, height, and cell size.
		/// </summary>
		/// <param name="width">The width of the grid.</param>
		/// <param name="height">The height of the grid.</param>
		/// <param name="cellSize">The size of each cell in the grid.</param>
		public GridSystem(int width, int height, float cellSize)
		{
			this._width = width;
			this._height = height;
			this._cellSize = cellSize;

			_gridObjectArray = new GridObject[_width, _height];
			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _height; z++)
				{
					GridPosition gridPosition = new GridPosition(x, z);
					_gridObjectArray[x, z] = new GridObject(this, gridPosition);
				}
			}
		}
		/// <summary>
		/// Converts a grid position to a world position.
		/// </summary>
		/// <param name="gridPosition">The position in the grid to be converted.</param>
		/// <returns>A Vector3 representing the world position corresponding to the given grid position.</returns>
		public UnityEngine.Vector3 GetWorldPosition(GridPosition gridPosition)
		{
			return new UnityEngine.Vector3(gridPosition.x, 0, gridPosition.z) * _cellSize;
		}
		
		/// <summary>
		/// Converts a world position to a grid position.
		/// </summary>
		/// <param name="worldPosition">The world position to convert.</param>
		/// <returns>A GridPosition representing the grid coordinates of the given world position.</returns>
		public GridPosition GetGridPosition(UnityEngine.Vector3 worldPosition)
		{
			return new GridPosition(
				Mathf.RoundToInt(worldPosition.x / _cellSize),
				Mathf.RoundToInt(worldPosition.z / _cellSize)
			);
		}

		public void CreateDebugObjects(Transform debugPrefab)
		{
			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _height; z++)
				{
					GridPosition gridPosition = new GridPosition(x, z);

					Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), UnityEngine.Quaternion.identity);
					GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
					gridDebugObject.SetGridObject(GetGridObject(gridPosition));
				}
			}
		}

		public GridObject GetGridObject(GridPosition gridPosition)
		{
			return _gridObjectArray[gridPosition.x, gridPosition.z];
		}

		public bool isValidGridPosition(GridPosition gridPosition)
		{
			return gridPosition.x >= 0 &&
					gridPosition.x < _width &&
					gridPosition.z >= 0 &&
					gridPosition.z < _height;
		}

		public int GetWidth()
		{
			return _width;
		}

		public int GetHeight()
		{
			return _height;
		}

	}
}