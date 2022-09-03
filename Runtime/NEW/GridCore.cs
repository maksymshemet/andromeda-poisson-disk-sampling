using System;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridCore
    {
        public IGridProperties Properties { get; }
        public Vector3 WorldPositionOffset { get; set;}
        public Vector3 Center => new Vector3(Properties.Size.x / 2, Properties.Size.y / 2) + WorldPositionOffset;

        private readonly int[] _cells;

        public GridCore(IGridProperties properties)
        {
            Properties = properties;

            _cells = new int[properties.CellHeight * properties.CellWidth];
        }

        public Vector2Int PositionToCellClamped(Vector3 worldPosition)
        {
            var x = Mathf.RoundToInt((worldPosition.x - WorldPositionOffset.x) / Properties.CellSize);
            var y = Mathf.RoundToInt((worldPosition.y - WorldPositionOffset.y) / Properties.CellSize);
            return new Vector2Int(
                x: Mathf.Max(0,Mathf.Min(x, Properties.CellWidth - 1)),
                y: Mathf.Max(0,Mathf.Min(y, Properties.CellHeight - 1)));
        }
        
        public Vector2Int PositionToCell(Vector3 worldPosition)
        {
            var x = Mathf.RoundToInt((worldPosition.x - WorldPositionOffset.x) / Properties.CellSize);
            var y = Mathf.RoundToInt((worldPosition.y - WorldPositionOffset.y) / Properties.CellSize);
            return new Vector2Int(x, y);
        }
        
        public Vector2Int PositionToCellFloor(Vector3 worldPosition)
        {
            var x = Mathf.FloorToInt((worldPosition.x - WorldPositionOffset.x) / Properties.CellSize);
            var y = Mathf.FloorToInt((worldPosition.y - WorldPositionOffset.y) / Properties.CellSize);
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

        public void ClearCell(int x, int y) =>
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
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / Properties.CellSize));

        public RegionCoordinates LookupRegion(Vector2Int sourceCell, int searchRange) =>
            RegionCoordinates.Create(searchSize: searchRange, sourceCoordinate: sourceCell);
        
        public RegionCoordinates LookupRegionClamped(Vector2Int sourceCell, int searchRange) =>
            RegionCoordinates.Create(
                searchSize: searchRange, 
                sourceCoordinate: sourceCell,
                startXLimit: 0, endXLimit: Properties.CellWidth,
                startYLimit: 0, endYLimit: Properties.CellHeight);
                
        public bool IsPointInAABB(Vector3 worldPosition)
        {
            return worldPosition.x >= WorldPositionOffset.x &&
                   worldPosition.y >= WorldPositionOffset.y &&
                   worldPosition.x <= WorldPositionOffset.x + Properties.Size.x &&
                   worldPosition.y <= WorldPositionOffset.y + Properties.Size.y;
        }
        
        public bool IsRegionInsideGrid(RegionCoordinates coordinates)
        {
            return coordinates.StartX >= 0 &&
                   coordinates.StartY >= 0 &&
                   coordinates.EndX < Properties.CellWidth &&
                   coordinates.EndY < Properties.CellHeight;
        }
        
        public bool IsCellInGrid(int cellX, int cellY)
        {
            return cellX >= 0 && cellX < Properties.CellWidth && cellY >= 0 && cellY < Properties.CellHeight;
        }
        
        public int FlatCoordinates(int x, int y) => y * Properties.CellWidth + x;
    }
}