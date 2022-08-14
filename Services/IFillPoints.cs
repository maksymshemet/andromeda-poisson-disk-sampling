using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public interface IFillPoints
    {
        public void FillPoints(in IGrid grid, Point point, in IWorld world = null, Vector2Int chunkPosition = default);
    }

    public class FillWorldGridPoints : IFillPoints
    {
        public void FillPoints(in IGrid grid, Point point, in IWorld world = null, Vector2Int chunkPosition = default)
        {
            
            if (point.WorldPosition.x < 0.52 && point.WorldPosition.x > 0.5 &&
                point.WorldPosition.y < 5.7 && point.WorldPosition.y > 5.6)
            {
                int a = 1;
            }
            
            var x = Mathf.FloorToInt((point.WorldPosition.x - grid.WorldPositionOffset.x) / grid.GridProperties.CellSize);
            var y = Mathf.FloorToInt((point.WorldPosition.y - grid.WorldPositionOffset.y) / grid.GridProperties.CellSize);

            var deltaRadius = Mathf.RoundToInt(point.Radius / grid.GridProperties.CellSize);
            
            var regionCoordinates = RegionCoordinates.Create(deltaRadius + 1, x, y);
            
            var sqrtRad = point.Radius * point.Radius;
            var pointCellValue = grid.GetCellValue(point.Cell);

            var externalCellValue = 0;
            
            var cachedIndexes = new Dictionary<Vector2Int, int>();
            
            for (int y1 = regionCoordinates.StartY; y1 < regionCoordinates.EndY; y1++)
            {
                var powYY = PowLengthBetweenCellPoints(y1, point.Cell.y, grid.GridProperties.CellSize);
                for (var x1 = regionCoordinates.StartX; x1 < regionCoordinates.EndX; x1++)
                {
                    if (x1 >= 0 && x1 < grid.GridProperties.CellWidth && y1 >= 0 && y1 < grid.GridProperties.CellHeight)
                    {   
                        var callValue = grid.GetCellValue(x1, y1);
                        if (callValue == 0)
                        {
                            var dstToCenter = PowLengthBetweenCellPoints(x1, point.Cell.x, grid.GridProperties.CellSize) + powYY;
                            var delta = dstToCenter - sqrtRad;
                            if (delta < grid.GridProperties.CellSize)
                            {
                                grid.SetCellValue(x1, y1, pointCellValue);
                            }
                        }
                    }
                    else
                    {
                        var dstToCenter = PowLengthBetweenCellPoints(x1, point.Cell.x, grid.GridProperties.CellSize) + powYY;
                        var delta = dstToCenter - sqrtRad;
                        if (delta < grid.GridProperties.CellSize)
                        {
                            var worldCoord = world.GetRealWorldCoordinate(chunkPosition, x1, y1);
                            if (cachedIndexes.TryGetValue(worldCoord.ChunkPosition, out var cachedValue))
                            {
                                world.SetCellValue(worldCoord, cachedValue);
                            }
                            else
                            {
                                var cellValue = world.AddPoint(worldCoord, point);
                                if(cellValue != 0)
                                    cachedIndexes[worldCoord.ChunkPosition] = cellValue;
                            }
                        } 
                    }
                }
            }
        }
        
        private float PowLengthBetweenCellPoints(int a, int b, float cellSize)
        {
            var deltaY = Mathf.Abs(a - b);
            var yy = (deltaY * cellSize);
            return yy * yy;
        }
    }

    public class FillGridPoints : IFillPoints
    {
        public void FillPoints(in IGrid grid, Point point, in IWorld world = null, Vector2Int chunkPosition = default)
        {
            var x = Mathf.FloorToInt((point.WorldPosition.x - grid.WorldPositionOffset.x) / grid.GridProperties.CellSize);
            var y = Mathf.FloorToInt((point.WorldPosition.y - grid.WorldPositionOffset.y) / grid.GridProperties.CellSize);

            var deltaRadius = Mathf.RoundToInt(point.Radius / grid.GridProperties.CellSize);
            
            var regionCoordinates = RegionCoordinates.Create(
                searchSizeStart: deltaRadius, 
                searchSizeEnd: deltaRadius + 1, 
                x: x, y: y,
                startXLimit: 0, endXLimit: grid.GridProperties.CellWidth,
                startYLimit: 0, endYLimit: grid.GridProperties.CellHeight);
            
            var sqrtRad = point.Radius * point.Radius;
            var pointCellValue = grid.GetCellValue(point.Cell);
            
            for (int y1 = regionCoordinates.StartY; y1 < regionCoordinates.EndY; y1++)
            {
                var coordStart = y1 * grid.GridProperties.CellWidth + regionCoordinates.StartX;
                var coordEnd = y1 * grid.GridProperties.CellWidth + regionCoordinates.EndX;
                var powYY = PowLengthBetweenCellPoints(y1, point.Cell.y, grid.GridProperties.CellSize);
                
                var x2 = regionCoordinates.StartX;
                for (var x1 = coordStart; x1 < coordEnd; x1++, x2++)
                {
                    if (grid.GetCellValue(x1) == 0)
                    {
                        var dstToCenter = PowLengthBetweenCellPoints(x2, point.Cell.x,
                            grid.GridProperties.CellSize) + powYY;
                        var delta = dstToCenter - sqrtRad;
                        if (delta < grid.GridProperties.CellSize)
                        {
                            grid.SetCellValue(x1, pointCellValue);
                        } 
                    }
                }
            }
        }
        
        private float PowLengthBetweenCellPoints(int a, int b, float cellSize)
        {
            var deltaY = Mathf.Abs(a - b);
            var yy = (deltaY * cellSize);
            return yy * yy;
        }
    }
}