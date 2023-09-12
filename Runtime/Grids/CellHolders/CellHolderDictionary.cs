using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class CellHolderDictionary : ICellHolder
    {
        public Vector2Int MinBound => new Vector2Int(int.MinValue, int.MinValue);
        public Vector2Int MaxBound => new Vector2Int(int.MaxValue, int.MaxValue);
        
        private readonly Dictionary<int, Dictionary<int, int>> _cells;

        public GridProperties GridProperties { get; }
        
        public CellHolderDictionary(GridProperties gridProperties)
        {
            GridProperties = gridProperties;
            _cells = new Dictionary<int, Dictionary<int, int>>();
        }

        public int GetCellValue(int x, int y)
        {
            if (_cells.TryGetValue(y, out Dictionary<int, int> rows))
            {
                if (rows.TryGetValue(x, out int cellValue))
                {
                    return cellValue;
                }
            }

            return 0;
        }

        public void SetCellValue(int x, int y, int value)
        {
            if (_cells.TryGetValue(y, out Dictionary<int, int> rows))
            {
                rows[x] = value;
            }
            else
            {
                _cells[y] = new Dictionary<int, int>
                {
                    { x, value }
                };
            }
        }

        public void ClearCellValue(int x, int y)
        {
            if (_cells.TryGetValue(y, out Dictionary<int, int> rows))
            {
                rows.Remove(x);
            }
        }

        public bool IsCellEmpty(int x, int y)
        {
            if (_cells.TryGetValue(y, out Dictionary<int, int> rows))
            {
                if (rows.TryGetValue(x, out int cellValue))
                {
                    return cellValue == 0;
                }
            }

            return true;
        }
        
        public Vector2Int CellFromWorldPosition(Vector3 worldPosition, WorldToCellPositionMethod method = WorldToCellPositionMethod.Round)
        {
            float x = (worldPosition.x - GridProperties.PositionOffset.x) / GridProperties.CellSize;
            float y = (worldPosition.y - GridProperties.PositionOffset.y) / GridProperties.CellSize;

            return method switch
            {
                WorldToCellPositionMethod.Round => new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)),
                WorldToCellPositionMethod.Ceil => new Vector2Int(Mathf.CeilToInt(x), Mathf.CeilToInt(y)),
                WorldToCellPositionMethod.Floor => new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y)),
                _ => throw new NotImplementedException($"method={method.ToString()} not implemented")
            };
        }

        public bool InBounds(Vector2Int cellCoordinate)
        {
            return true;
        }
        
        public bool IsPositionInAABB(Vector3 worldPosition)
        {
            return true;
        }

        public void Clear()
        {
            _cells.Clear();
        }
        
        public IEnumerable<CellValue> Values()
        {
            foreach (KeyValuePair<int, Dictionary<int, int>> rows in _cells)
            {
                foreach (KeyValuePair<int, int> cells in _cells[rows.Key])
                {
                    yield return new CellValue
                    {
                        X = cells.Key,
                        Y = rows.Key,
                        Value = cells.Value
                    };
                }
            }
        }
    }
}