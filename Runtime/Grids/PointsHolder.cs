using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class PointsHolder : IGrid
    {
        public event Action<IGrid, Point> OnPointCreated;
        public event Action<IGrid, Point> OnPointRemoved;
        
        public ICellHolder Cells { get; }
        public GridProperties GridProperties { get; }
        public IEnumerable<Point> Points => GetPointsIEnumerable();
        public int PointsCount => _points.Count;

        private Queue<int> _emptyPointIndexes;
        
        private readonly IRadiusSelector _radiusSelector;
        private readonly ICandidateValidator _candidateValidator;
        private readonly List<Point> _points;

        public PointsHolder(ICellHolder cells, ICandidateValidator candidateValidator, GridProperties gridProperties, IRadiusSelector radiusSelector)
        {
            Cells = cells;
            GridProperties = gridProperties;
            _candidateValidator = candidateValidator;
            _radiusSelector = radiusSelector;

            _points = new List<Point>();
        }
        
        public Point GetPoint(int x, int y)
        {
            int index = Cells.GetCellValue(x, y) - 1;
            return index > -1 ? _points[index] : default;
        }
        
        public Point GetPointByIndex(int pointIndex) => _points[pointIndex - 1];

        public void RemovePoint(Point point)
        {
            int pointIndex = _points.IndexOf(point);
            if(pointIndex < 0) return;
            
            _points[pointIndex] = null;
            
            int pointCellIndex = pointIndex + 1;

            if (GridProperties.FillCellsInsidePoint)
            {
                int searchSize = GetSearchSize(point.Radius);

                SearchBoundaries searchBoundaries = Helper.GetSearchBoundaries(this, point.CellMin, point.CellMax, searchSize);
            
                for (int y = searchBoundaries.StartY; y <= searchBoundaries.EndY; y++)
                {
                    for (int x = searchBoundaries.StartX; x <= searchBoundaries.EndX; x++)
                    {
                        int cellValue = Cells.GetCellValue(x, y);
                        if (cellValue == pointCellIndex)
                        {
                            Cells.ClearCellValue(x, y);
                        }
                    }
                }
            }
            else
            {
                Cells.ClearCellValue(point.CellMin.x, point.CellMin.y);
                Cells.ClearCellValue(point.CellMax.x, point.CellMax.y);
                Cells.ClearCellValue(point.CellMin.x, point.CellMax.y);
                Cells.ClearCellValue(point.CellMax.x, point.CellMin.y);
            }
            
            if (_emptyPointIndexes == null)
            {
                _emptyPointIndexes = new Queue<int>();
            }
            
            _emptyPointIndexes.Enqueue(pointIndex);
            
            OnPointRemoved?.Invoke(this, point);
        }
        
        public Point TrySpawnPointFrom(Point point)
        {
            for (var i = 0; i < GridProperties.Tries; i++)
            {
                PointSize candidateSize = _radiusSelector.GetRadius(i, GridProperties.Tries);
                Candidate candidate = CreateCandidateFrom(point, candidateSize);
                
                Point newPoint = TryAddPoint(candidate);
                if (newPoint != null)
                {
                    return newPoint;
                }
            }
        
            return null;
        }

        public Point TryAddPoint(Candidate candidate)
        {
            if (IsCandidateInAABB(candidate))
            {
                int searchSize = GetSearchSize(candidate.Radius + candidate.Margin);
                
                if (_candidateValidator.IsValid(this, candidate, searchSize))
                {
                    var newPoint = new Point(worldPosition: candidate.WorldPosition,
                        radius: candidate.Radius, margin: candidate.Margin)
                    {
                        CellMin = Cells.CellFromWorldPosition(
                            worldPosition: candidate.WorldPosition,
                            method: WorldToCellPositionMethod.Floor),
                        CellMax = Cells.CellFromWorldPosition(
                            worldPosition: candidate.WorldPosition,
                            method: WorldToCellPositionMethod.Ceil)
                    };

                    StorePoint(newPoint);
                    return newPoint;
                }
            }

            return null;
        }

        public HashSet<Point> GetPointsAround(in Point point, int region)
        {
            HashSet<Point> set = GetPointsAround(point.CellMin, point.CellMax, region);
            set.Remove(point);
            return set;
        }

        public HashSet<Point> GetPointsAround(in Vector2Int cellMin, in Vector2Int cellMax, int region)
        {
            var from = new Vector2Int(Mathf.Max(Cells.MinBound.x, cellMin.x - region),
                Mathf.Max(Cells.MinBound.y, cellMin.y - region));
            var to = new Vector2Int(Mathf.Min(Cells.MaxBound.x, cellMax.x + region),
                Mathf.Min(Cells.MaxBound.y, cellMax.y + region));

            var result = new HashSet<Point>();

            for (int y = from.y; y < to.y; y++)
            {
                for (int x = from.x; x < to.x; x++)
                {
                    if(Cells.IsCellEmpty(x, y)) continue;

                    Point point = _points[Cells.GetCellValue(x, y) - 1];
                    result.Add(point);
                }
            }
            
            return result;
        }

        public Vector2 CellToWorldCoordinate(in Vector2Int cell)
        {
            return CellToWorldCoordinate(cell.x, cell.y);
        }

        public Vector2 CellToWorldCoordinate(int x, int y)
        {
            float x1 = GridProperties.CellSize * x + GridProperties.PositionOffset.x;
            float y1 = GridProperties.CellSize * y + GridProperties.PositionOffset.y;
            return new Vector2(x1, y1);
        }

        public void Clear()
        {
            Cells.Clear();
            
            _points.Clear();
            _emptyPointIndexes?.Clear();
        }

        protected virtual bool IsCandidateInAABB(in Candidate candidate)
        {
            if (GridProperties.PointsLocation == PointsLocation.CenterInsideGrid)
            {
                return Cells.IsPositionInAABB(candidate.WorldPosition);
            }

            if (GridProperties.PointsLocation == PointsLocation.PointInsideGrid)
            {
                return Cells.IsPositionInAABB(new Vector3(
                           candidate.WorldPosition.x - candidate.Radius,
                           candidate.WorldPosition.y - candidate.Radius)) &&
                       Cells.IsPositionInAABB(new Vector3(
                           candidate.WorldPosition.x + candidate.Radius,
                           candidate.WorldPosition.y + candidate.Radius));
            }
            
            return Cells.IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x - candidate.Radius + candidate.Margin,
                       candidate.WorldPosition.y - candidate.Radius + candidate.Margin)) &&
                   Cells.IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x + candidate.Radius + candidate.Margin,
                       candidate.WorldPosition.y + candidate.Radius + candidate.Margin));
        }
        
        protected virtual void StorePoint(Point point)
        {
            int pointIndex;
            
            if (_emptyPointIndexes is { Count: > 0 })
            {
                pointIndex = _emptyPointIndexes.Dequeue();
                _points[pointIndex] = point;
            }
            else
            {
                _points.Add(point);
                pointIndex = _points.Count - 1;
            }
            
            point.Index = pointIndex;
            
            FillGridValues(point, pointIndex + 1);
            
            OnPointCreated?.Invoke(this, point);
        }
        
        private void FillGridValues(in Point point, int pointCellIndex)
        {
            if (GridProperties.FillCellsInsidePoint)
            {
                int searchSize = Mathf.RoundToInt((point.Radius + point.Margin) / GridProperties.CellSize);
                SearchBoundaries searchBoundaries = Helper.GetSearchBoundaries(this, point.CellMin, point.CellMax, searchSize);
            
                float sqrtRad = Mathf.Pow(point.Radius + point.Margin, 2);
                for (int y = searchBoundaries.StartY; y <= searchBoundaries.EndY; y++)
                {
                    float cellY = GridProperties.CellSize * y + GridProperties.PositionOffset.y;
                    for (int x = searchBoundaries.StartX; x <= searchBoundaries.EndX; x++)
                    {
                        if (Cells.GetCellValue(x, y) == 0)
                        {
                            float cellX = GridProperties.CellSize * x + GridProperties.PositionOffset.x;
                            if (IsInsideCircle(point, sqrtRad, cellX, cellY))
                            {
                                Cells.SetCellValue(x, y, pointCellIndex);
                            } 
                        }
                    }
                }
            }
            else
            {
                Cells.SetCellValue(point.CellMin.x, point.CellMin.y, pointCellIndex);
                Cells.SetCellValue(point.CellMax.x, point.CellMax.y, pointCellIndex);
                Cells.SetCellValue(point.CellMin.x, point.CellMax.y, pointCellIndex);
                Cells.SetCellValue(point.CellMax.x, point.CellMin.y, pointCellIndex);
            }
        }
        
        private bool IsInsideCircle(Point point, float sqrtRad, float x, float y)
        {
            double dx = x - point.WorldPosition.x;
            double dy = y - point.WorldPosition.y;
            double distanceSquared = dx * dx + dy * dy;
            return distanceSquared < sqrtRad;
        }
        
        private Candidate CreateCandidateFrom(Point point, PointSize candidateSize)
        {
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: point.Radius + point.Margin,
                    candidateRadius: candidateSize.Radius + candidateSize.Margin + GridProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = candidateSize.Radius,
                Margin = candidateSize.Margin + GridProperties.PointMargin
            };
        }
        
        private int GetSearchSize(float pointRadius) => 
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridProperties.CellSize));

        private IEnumerable<Point> GetPointsIEnumerable()
        {
            foreach (Point point in _points)
            {
                if(point != null)
                    yield return point;
            }
        }
    }
}