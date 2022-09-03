using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridWorldMultiRad : GridWorld
    {
        public GridWorldMultiRad(GridCore gridCore, World world, 
            Vector2Int chunkPosition) : base(gridCore, world, chunkPosition)
        {
        }

        protected override int GetSearchRange(float pointRadius)
        {
            return Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridCore.Properties.CellSize));
        }

        public override bool TryAddPoint(PointWorld point)
        {
            if (!base.TryAddPoint(point)) return false;
            
            var cell = GridCore.PositionToCellFloor(point.WorldPosition);
            var deltaRadius = Mathf.RoundToInt(point.Radius / GridCore.Properties.CellSize);
            var lookupRegion = GridCore.LookupRegion(cell, deltaRadius + 1);
            var sqrtPointRadius = point.Radius * point.Radius;
            var pointCellValue = GridCore.GetCellValue(point.Cell.x, point.Cell.y);
          
            for (int y1 = lookupRegion.StartY; y1 < lookupRegion.EndY; y1++)
            {
                var powYY = Helper.PowLengthBetweenCellPoints(y1, point.Cell.y, GridCore.Properties.CellSize);
                for (var x1 = lookupRegion.StartX; x1 < lookupRegion.EndX; x1++)
                {
                    if (GridCore.IsCellInGrid(x1, y1))
                    {
                        MarkCellsInsideGrid(x1, y1, sqrtPointRadius, 
                            point.Cell.x, powYY, pointCellValue);
                    }
                    else
                    {
                        MarkCellsOutsideGrid(x1, y1, sqrtPointRadius, powYY, point);
                    }
                }
            }

            return true;
        }

        public override bool RemovePoint(PointWorld point)
        {
            var cellValue = GridCore.GetCellValue(point.Cell.x, point.Cell.y);
            if (base.RemovePoint(point))
            {
                var searchRange = GetSearchRange(point.Radius);
                var regionCoordinates = GridCore.LookupRegion(point.Cell, searchRange);

                for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
                {
                    for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                    {
                        if (GridCore.IsCellInGrid(x, y) && GridCore.GetCellValue(x, y) == cellValue)
                        {
                            GridCore.ClearCell(x, y);
                        }
                        else
                        {
                            var worldCoordinate = World.GetRealWorldCoordinate(ChunkPosition, x, y);
                            World.RemoveLink(worldCoordinate, point);
                        }
                    }
                }
            }

            return false;
        }

        private void MarkCellsInsideGrid(int x, int y, float sqrtRad, int pointCellX, float powYY, int pointCellValue)
        {
            var callValue = GridCore.GetCellValue(x, y);
            if (callValue == 0)
            {
                var dstToCenter = Helper.PowLengthBetweenCellPoints(x, pointCellX, GridCore.Properties.CellSize) + powYY;
                var delta = dstToCenter - sqrtRad;
                if (delta < GridCore.Properties.CellSize)
                {
                    GridCore.SetCellValue(x, y, pointCellValue);
                }
            }
        }

        private void MarkCellsOutsideGrid(int x, int y, float sqrtRad, float powYY, PointWorld point)
        {
            var dstToCenter = Helper.PowLengthBetweenCellPoints(x, point.Cell.x, GridCore.Properties.CellSize) + powYY;
            var delta = dstToCenter - sqrtRad;
            if (delta < GridCore.Properties.CellSize)
            {
                var worldCoord = World.GetRealWorldCoordinate(ChunkPosition, x, y);
                World.LinkPoint(worldCoord, point);
            }
        }
    }
}