using System;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridCore
    {
        public readonly Vector2Int Size;
        public readonly float CellSize;
        public readonly int CellWidth;
        public readonly int CellHeight;

        public Vector3 WorldPositionOffset { get; set;}
        
        private readonly int[] _cells;

        public GridCore(GridProperties gridProperties)
        {
            Size = gridProperties.Size;
            CellSize = gridProperties.CellSize;
            CellWidth = gridProperties.CellWidth;
            CellHeight = gridProperties.CellHeight;

            _cells = new int[CellHeight * CellWidth];
        }

        public Vector2Int PositionToCellClamped(Vector3 worldPosition)
        {
            var x = Mathf.RoundToInt((worldPosition.x - WorldPositionOffset.x) / CellSize);
            var y = Mathf.RoundToInt((worldPosition.y - WorldPositionOffset.y) / CellSize);
            return new Vector2Int(
                x: Mathf.Max(0,Mathf.Min(x, CellWidth - 1)),
                y: Mathf.Max(0,Mathf.Min(y, CellHeight - 1)));
        }
        
        public Vector2Int PositionToCell(Vector3 worldPosition)
        {
            var x = Mathf.RoundToInt((worldPosition.x - WorldPositionOffset.x) / CellSize);
            var y = Mathf.RoundToInt((worldPosition.y - WorldPositionOffset.y) / CellSize);
            return new Vector2Int(x, y);
        }
        
        public Vector2Int PositionToCellFloor(Vector3 worldPosition)
        {
            var x = Mathf.FloorToInt((worldPosition.x - WorldPositionOffset.x) / CellSize);
            var y = Mathf.FloorToInt((worldPosition.y - WorldPositionOffset.y) / CellSize);
            return new Vector2Int(x, y);
        }

        public int GetCellValue(int x, int y)
        {
            return GetCellValue(FlatCoordinates(x, y));
        }
        
        public int GetCellValue(int index) =>
            _cells[index];
        
        public void SetCellValue(int x, int y, int value) => 
            SetCellValue(FlatCoordinates(x, y), value);

        public void SetCellValue(int index, int value) =>
            _cells[index] = value;

        public void CleatCell(int x, int y) =>
            _cells[FlatCoordinates(x, y)] = 0;
        
        public bool IsCellEmpty(int x, int y) =>
            IsCellEmpty(FlatCoordinates(x, y));
        
        public bool IsCellEmpty(int index)
        {
            try
            {
                return _cells[index] == 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int SearchRange(float pointRadius) =>
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / CellSize));

        public RegionCoordinates LookupRegion(Vector2Int sourceCell, int searchRange) =>
            RegionCoordinates.Create(searchSize: searchRange, sourceCoordinate: sourceCell);
        
        public RegionCoordinates LookupRegionClamped(Vector2Int sourceCell, int searchRange) =>
            RegionCoordinates.Create(
                searchSize: searchRange, 
                sourceCoordinate: sourceCell,
                startXLimit: 0, endXLimit: CellWidth,
                startYLimit: 0, endYLimit: CellHeight);
                
        public bool IsPointInAABB(Vector3 worldPosition)
        {
            return worldPosition.x >= WorldPositionOffset.x &&
                   worldPosition.y >= WorldPositionOffset.y &&
                   worldPosition.x <= WorldPositionOffset.x + Size.x &&
                   worldPosition.y <= WorldPositionOffset.y + Size.y;
        }

        public bool IsCellInGrid(int cellX, int cellY)
        {
            return cellX >= 0 && cellX < CellWidth && cellY >= 0 && cellY < CellHeight;
        }
        
        public int FlatCoordinates(int x, int y) => y * CellWidth + x;
    }
}