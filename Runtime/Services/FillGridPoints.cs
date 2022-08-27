using System.Collections.Generic;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class FillGridPoints : IFillPoints<IGrid, Point>
    {
        public Dictionary<Vector2Int, int> FillPoints(in IGrid grid, Point point)
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

            return null;
        }
        
        private float PowLengthBetweenCellPoints(int a, int b, float cellSize)
        {
            var deltaY = Mathf.Abs(a - b);
            var yy = (deltaY * cellSize);
            return yy * yy;
        }
    }
}