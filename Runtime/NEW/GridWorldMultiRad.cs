using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridWorldMultiRad : GridWorld
    {
        private readonly Dictionary<int, Dictionary<Vector2Int, int>> _cachedPoints;

        public GridWorldMultiRad(GridCore gridCore, WorldMultiRad world, 
            Vector2Int chunkPosition, IRadius radius) : base(gridCore, world, chunkPosition, radius)
        {
            _cachedPoints = new Dictionary<int, Dictionary<Vector2Int, int>>();
        }

        protected override bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries,
            out Candidate candidate)
        {
            var radius = radiusProvider.GetRadius(currentTry, maxTries);
            if (radius == 0)
            {
                candidate = default;
                return false;
            }

            var position = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: spawnerPosition,
                    spawnerRadius: spawnerRadius,
                    candidateRadius: radius);
            
            if(!GridCore.IsPointInAABB(position))
            {
                candidate = default;
                return false;
            }
            
            candidate = new Candidate
            {
                Radius = radius,
                WorldPosition = position,
                Cell = GridCore.PositionToCellClamped(position)

            };
            
            return true;
        }

        protected override int GetSearchRange(float pointRadius)
        {
            return Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridCore.CellSize));
        }

        protected override void PostPointCreated(in PointWorld point, int pointIndex)
        {
            var cell = GridCore.PositionToCellFloor(point.WorldPosition);
            var deltaRadius = Mathf.RoundToInt(point.Radius / GridCore.CellSize);
            var lookupRegion = GridCore.LookupRegion(cell, deltaRadius + 1);
            var sqrtPointRadius = point.Radius * point.Radius;
            var pointCellValue = GridCore.GetCellValue(point.Cell.x, point.Cell.y);

          
            for (int y1 = lookupRegion.StartY; y1 < lookupRegion.EndY; y1++)
            {
                var powYY = Helper.PowLengthBetweenCellPoints(y1, point.Cell.y, GridCore.CellSize);
                for (var x1 = lookupRegion.StartX; x1 < lookupRegion.EndX; x1++)
                {
                    if (GridCore.IsCellInGrid(x1, y1))
                    {
                        MarkCellsInsideGrid(x1, y1, sqrtPointRadius, 
                            point.Cell.x, powYY, pointCellValue);
                    }
                    else
                    {
                        MarkCellsOutsideGrid(x1, y1, sqrtPointRadius, powYY, point, pointIndex);
                    }
                }
            }
        }

        protected override void OnPointRemoved(PointWorld point, int pointIndex, int cellValue)
        {
            var searchRange = GetSearchRange(point.Radius);
            var regionCoordinates = GridCore.LookupRegion(point.Cell, searchRange);

            for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
            {
                for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                {
                    if (GridCore.IsCellInGrid(x, y) && GridCore.GetCellValue(x, y) == cellValue)
                    {
                        GridCore.CleatCell(x, y);
                    }
                    else
                    {
                        if (_cachedPoints.TryGetValue(pointIndex, out var cachedCellValues))
                        {
                            if (cachedCellValues.TryGetValue(new Vector2Int(x, y), out var cachedValue))
                            {
                                var worldCoordinate = World.GetRealWorldCoordinate(ChunkPosition, x, y);
                                if (World.GetGrid(worldCoordinate.ChunkPosition).GridCore
                                        .GetCellValue(worldCoordinate.CellX, worldCoordinate.CellY) == cachedValue)
                                {
                                    World.ClearCell(worldCoordinate);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MarkCellsInsideGrid(int x, int y, float sqrtRad, int pointCellX, float powYY, int pointCellValue)
        {
            var callValue = GridCore.GetCellValue(x, y);
            if (callValue == 0)
            {
                var dstToCenter = Helper.PowLengthBetweenCellPoints(x, pointCellX, GridCore.CellSize) + powYY;
                var delta = dstToCenter - sqrtRad;
                if (delta < GridCore.CellSize)
                {
                    GridCore.SetCellValue(x, y, pointCellValue);
                }
            }
        }

        private void MarkCellsOutsideGrid(int x, int y, float sqrtRad, float powYY, PointWorld point,
            int pointIndex)
        {
            if (!_cachedPoints.TryGetValue(pointIndex, out var cachedCellValues))
            {
                cachedCellValues = new Dictionary<Vector2Int, int>();
                _cachedPoints[pointIndex] = cachedCellValues;
            }

            var dstToCenter = Helper.PowLengthBetweenCellPoints(x, point.Cell.x, GridCore.CellSize) + powYY;
            var delta = dstToCenter - sqrtRad;
            if (delta < GridCore.CellSize)
            {
                var worldCoord = World.GetRealWorldCoordinate(ChunkPosition, x, y);
                if(cachedCellValues.TryGetValue(worldCoord.ChunkPosition, out var cachedCellValue))
                {
                    World.GetGrid(worldCoord.ChunkPosition)
                        .GridCore.SetCellValue(worldCoord.CellX, worldCoord.CellY, cachedCellValue);
                }
                else
                {
                    var status = World.TryAddPoint(worldCoord, point, cache: true);
                    if (status == PointWorldStatus.Added)
                    {
                        var cellValue = World.GetGrid(worldCoord.ChunkPosition)
                            .GridCore.GetCellValue(worldCoord.CellX, worldCoord.CellY);
                        cachedCellValues[worldCoord.ChunkPosition] = cellValue;
                    }
                }
            }
        }
    }
}