using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class CellHolderArray : ICellHolder
    {
        public Vector2Int MinBound => Vector2Int.zero;
        public Vector2Int MaxBound => new Vector2Int(
            x: _cells.GetLength(1), 
            y: _cells.GetLength(0)
        );

        private readonly int[,] _cells;
        public GridProperties GridProperties { get; }
        
        public CellHolderArray(GridProperties gridProperties)
        {
            GridProperties = gridProperties;
            
            _cells = new int[gridProperties.CellLenghtY, gridProperties.CellLenghtX];
        }
        
        public int GetCellValue(int x, int y) => _cells[y, x];

        public void SetCellValue(int x, int y, int value) => _cells[y, x] = value;

        public void ClearCellValue(int x, int y) => _cells[y, x] = 0;
        
        public bool IsCellEmpty(int x, int y) => _cells[y, x] == 0;
        
        public Vector2Int CellFromWorldPosition(Vector3 worldPosition, WorldToCellPositionMethod method = WorldToCellPositionMethod.Round)
        {
            float x = (worldPosition.x - GridProperties.PositionOffset.x) / GridProperties.CellSize;
            float y = (worldPosition.y - GridProperties.PositionOffset.y) / GridProperties.CellSize;

            Vector2Int cellPosition = method switch
            {
                WorldToCellPositionMethod.Round => new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)),
                WorldToCellPositionMethod.Ceil => new Vector2Int(Mathf.CeilToInt(x), Mathf.CeilToInt(y)),
                WorldToCellPositionMethod.Floor => new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y)),
                _ => throw new NotImplementedException($"method={method.ToString()} not implemented")
            };

            return new Vector2Int(
                x: Mathf.Max(MinBound.x, Mathf.Min(MaxBound.x - 1, cellPosition.x)),
                y: Mathf.Max(MinBound.y, Mathf.Min(MaxBound.y - 1, cellPosition.y)));
        }

        public bool InBounds(Vector2Int cellCoordinate)
        {
            return cellCoordinate.x < MaxBound.x ||
                   cellCoordinate.y < MaxBound.y ||
                   cellCoordinate.x >= 0 ||
                   cellCoordinate.y >= 0;
        }
        
        public bool IsPositionInAABB(Vector3 worldPosition)
        {
            return worldPosition.x >= GridProperties.PositionOffset.x &&
                   worldPosition.y >= GridProperties.PositionOffset.y &&
                   worldPosition.x <= GridProperties.PositionOffset.x + GridProperties.Size.x &&
                   worldPosition.y <= GridProperties.PositionOffset.y + GridProperties.Size.y;
        }

        public void Clear()
        {
            for (var y = 0; y < _cells.GetLength(0); y++)
            {
                for (var x = 0; x < _cells.GetLength(1); x++)
                {
                    _cells[y, x] = 0;
                }
            }
        }

        public IEnumerable<CellValue> Values()
        {
            for (var y = 0; y < _cells.GetLength(0); y++)
            {
                for (var x = 0; x < _cells.GetLength(1); x++)
                {
                    yield return new CellValue
                    {
                        X = x,
                        Y = y,
                        Value = _cells[y, x]
                    };
                }
            }
        }
    }
}