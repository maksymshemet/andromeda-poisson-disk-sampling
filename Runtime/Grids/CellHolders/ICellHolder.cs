using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders
{
    public interface ICellHolder
    {
        public Vector2Int MinBound { get; }
        public Vector2Int MaxBound { get; }
        
        int GetCellValue(int x, int y);
        
        void SetCellValue(int x, int y, int value);
        
        void ClearCellValue(int x, int y);
        
        bool IsCellEmpty(int x, int y);

        Vector2Int CellFromWorldPosition(Vector3 worldPosition, WorldToCellPositionMethod method = WorldToCellPositionMethod.Round);

        bool InBounds(Vector2Int cellCoordinate);

        bool IsPositionInAABB(Vector3 worldPosition);
        
        void Clear();

        IEnumerable<CellValue> GetValues();
    }
}