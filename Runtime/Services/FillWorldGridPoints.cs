using System.Collections.Generic;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class FillWorldGridPoints : IFillPoints<IWorldGrid, PointWorld>
    {
        public void FillPoints(in IWorldGrid grid, PointWorld point)
        {
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
                            var worldCoord = grid.World.GetRealWorldCoordinate(grid.ChunkPosition, x1, y1);
                            if (cachedIndexes.TryGetValue(worldCoord.ChunkPosition, out var cachedValue))
                            {
                                grid.World.GetGrid(worldCoord.ChunkPosition)
                                    .SetCellValue(worldCoord.CellX, worldCoord.CellY, cachedValue);
                            }
                            else
                            {
                                var cellValue = grid.World.AddPoint(worldCoord, point);
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
}