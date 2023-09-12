using System;
using System.Collections.Generic;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface ICandidateValidator<TPoint> where TPoint : PointGrid, new()
    {
        public bool IsValid(IGrid<TPoint> grid, Candidate candidate, int searchSize);
    }

    public class DefaultCandidateValidator<TPoint> : ICandidateValidator<TPoint> where TPoint : PointGrid, new()
    {
        public virtual bool IsValid(IGrid<TPoint> grid, Candidate candidate, int searchSize)
        {
            int startX = Mathf.Max(grid.Cells.MinBound.x, candidate.CellMin.x - searchSize);
            int endX = Mathf.Min(candidate.CellMax.x + searchSize, grid.Cells.MaxBound.x - 1);
            int startY = Mathf.Max(grid.Cells.MinBound.y, candidate.CellMin.y - searchSize);
            int endY = Mathf.Min(candidate.CellMax.y + searchSize, grid.Cells.MaxBound.y - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    int pointIndex = grid.Cells.GetCellValue(x, y);
                    if (pointIndex == 0) continue;

                    PointGrid existingPoint = grid.GetPointByIndex(pointIndex);
                    if (candidate.IsIntersectWithPoint(existingPoint))
                    {
                        return false;
                    } 
                }
            }

            return true;
        }
    }

    public abstract class PointsHolder<TPoint, TPointProperty, TSelf> : IGrid<TPoint>
        where TPoint : PointGrid, new()
        where TPointProperty : PointProperties
        where TSelf : PointsHolder<TPoint, TPointProperty, TSelf>
    {
        public event Action<TSelf, TPoint> OnPointCreated;
        
        public ICellHolder Cells { get; }
        
        public ICandidateValidator<TPoint> CandidateValidator { get; }
        public GridProperties GridProperties { get; }
        public TPointProperty PointProperties { get; }
        
        public ICustomPointBuilder<TPoint> CustomBuilder { get; set; }
        
        public IEnumerable<TPoint> Points => _points.Where(x => x != null);
        
        public int PointCount => _points.Count;

        private Queue<int> _emptyPointIndexes;

        private readonly List<TPoint> _points;

        protected PointsHolder(ICellHolder cells, ICandidateValidator<TPoint> candidateValidator, GridProperties gridProperties, TPointProperty pointProperties)
        {
            Cells = cells;
            GridProperties = gridProperties;
            PointProperties = pointProperties;
            CandidateValidator = candidateValidator;
            
            _points = new List<TPoint>();
        }

        public int IndexOf(in TPoint point)
        {
            return _points.IndexOf(point);
        }
        
        public TPoint GetPoint(int x, int y)
        {
            int index = Cells.GetCellValue(x, y) - 1;
            return index > -1 ? _points[index] : default;
        }

        public void RemovePoint(TPoint point)
        {
            int pointIndex = _points.IndexOf(point);
            if(pointIndex < 0) return;
            
            _points[pointIndex] = null;
            
            int pointCellIndex = pointIndex + 1;
            
            int searchSize = GetSearchSize(point.Radius);
            
            int startX = Mathf.Max(Cells.MinBound.x, point.CellMin.x - searchSize);
            int endX = Mathf.Min(point.CellMax.x + searchSize, Cells.MaxBound.x - 1);
            int startY = Mathf.Max(Cells.MinBound.y, point.CellMin.y - searchSize);
            int endY = Mathf.Min(point.CellMax.y + searchSize, Cells.MaxBound.y - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    int cellValue = Cells.GetCellValue(x, y);
                    if (cellValue == pointCellIndex)
                    {
                        Cells.ClearCellValue(x, y);
                    }
                }
            }

            if (_emptyPointIndexes == null)
            {
                _emptyPointIndexes = new Queue<int>();
            }
            
            _emptyPointIndexes.Enqueue(pointIndex);
        }
        
        public int TrySpawnPointFrom(TPoint point, out TPoint newPoint)
        {
            for (var i = 0; i < PointProperties.Tries; i++)
            {
                float candidateRadius = CreateCandidateRadius(i, PointProperties.Tries);
                Candidate candidate = CreateCandidateFrom(point, candidateRadius);

                if (IsCandidateInAABB(candidate))
                {
                    candidate.CellMax = Cells.CellFromWorldPosition(candidate.WorldPosition, 
                        WorldToCellPositionMethod.Ceil);
                    candidate.CellMin = Cells.CellFromWorldPosition(candidate.WorldPosition, 
                        WorldToCellPositionMethod.Floor);

                    // if (IsCandidateValid(candidate))
                    if (CandidateValidator.IsValid(this, candidate, GetSearchSize(candidate.Radius)))
                    {
                        newPoint = new TPoint
                        {
                            WorldPosition = candidate.WorldPosition,
                            Radius = candidate.Radius,
                            Margin = candidate.Margin
                        };
                        
                        newPoint.CellMin = Cells.CellFromWorldPosition(
                            worldPosition: newPoint.WorldPosition, 
                            method: WorldToCellPositionMethod.Floor);
            
                        newPoint.CellMax = Cells.CellFromWorldPosition(
                            worldPosition: newPoint.WorldPosition, 
                            method: WorldToCellPositionMethod.Ceil);
                        
                        if (CustomBuilder != null)
                        {
                            if (!CustomBuilder.Build(  this, newPoint, i, PointProperties.Tries))
                            {
                                continue;
                            }
                        }
                        
                        StorePoint(newPoint);
                        return _points.Count - 1;
                    }
                }
            }

            newPoint = default;
            return -1;
        }

        public bool AddPoint(TPoint point)
        {
            var candidate = new Candidate
            {
                Radius = point.Radius,
                Margin = point.Margin,
                WorldPosition = point.WorldPosition
            };
        
            if (IsCandidateInAABB(candidate))
            {
                candidate.CellMax = Cells.CellFromWorldPosition(candidate.WorldPosition,
                    WorldToCellPositionMethod.Ceil);
                candidate.CellMin = Cells.CellFromWorldPosition(candidate.WorldPosition,
                    WorldToCellPositionMethod.Floor);
        
                if (IsCandidateValid(candidate))
                {
                    point.CellMin = Cells.CellFromWorldPosition(
                        worldPosition: point.WorldPosition,
                        method: WorldToCellPositionMethod.Floor);
        
                    point.CellMax = Cells.CellFromWorldPosition(
                        worldPosition: point.WorldPosition,
                        method: WorldToCellPositionMethod.Ceil);
        
                    if (CustomBuilder != null)
                    {
                        if (!CustomBuilder.Build(this, point, 0, PointProperties.Tries))
                        {
                            return false;
                        }
                    }
        
                    StorePoint(point);
                    
                    return true;
                }
            }
        
            return false;
        }
        
        public HashSet<TPoint> GetPointsAround(in TPoint pointWorld, int region)
        {
            HashSet<TPoint> set = GetPointsAround(pointWorld.CellMin, pointWorld.CellMax, region);
            set.Remove(pointWorld);
            return set;
        }
        
        public HashSet<TPoint> GetPointsAround(in Vector2Int cellMin, in Vector2Int cellMax, int region)
        {
            var from = new Vector2Int(Mathf.Max(Cells.MinBound.x, cellMin.x - region),
                Mathf.Max(Cells.MinBound.y, cellMin.y - region));
            var to = new Vector2Int(Mathf.Min(Cells.MaxBound.x, cellMax.x + region),
                Mathf.Min(Cells.MaxBound.y, cellMax.y + region));

            var result = new HashSet<TPoint>();

            for (int y = from.y; y < to.y; y++)
            {
                for (int x = from.x; x < to.x; x++)
                {
                    if(Cells.IsCellEmpty(x, y)) continue;

                    TPoint point = _points[Cells.GetCellValue(x, y) - 1];
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
            _points.Clear();
            Cells.Clear();
        }
        
        protected bool IsCandidateInAABB(in Candidate candidate)
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
                       candidate.WorldPosition.x - candidate.FullRadius,
                       candidate.WorldPosition.y - candidate.FullRadius)) &&
                   Cells.IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x + candidate.FullRadius,
                       candidate.WorldPosition.y + candidate.FullRadius));
        }
        
        protected abstract float CreateCandidateRadius(int currentTry, int maxTries);
        
        public TPoint GetPointByIndex(int pointIndex) => _points[pointIndex - 1];
        
        protected int StorePoint(TPoint point)
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
            
            OnPointCreatedInternal(point, pointIndex + 1);
            
            OnPointCreated?.Invoke((TSelf) this, point);
            
            return pointIndex;
        }
        
        protected virtual void OnPointCreatedInternal(in TPoint point, int pointCellIndex)
        {
            Cells.SetCellValue(point.CellMin.x, point.CellMin.y, pointCellIndex);
            Cells.SetCellValue(point.CellMax.x, point.CellMax.y, pointCellIndex);
            Cells.SetCellValue(point.CellMin.x, point.CellMax.y, pointCellIndex);
            Cells.SetCellValue(point.CellMax.x, point.CellMin.y, pointCellIndex);

            if (GridProperties.FillCellsInsidePoint)
            {
                int searchSize = Mathf.RoundToInt((point.FullRadius) / GridProperties.CellSize);
                
                int startX = Mathf.Max(Cells.MinBound.x, point.CellMin.x - searchSize);
                int endX = Mathf.Min(point.CellMax.x + searchSize, Cells.MaxBound.x - 1);
                int startY = Mathf.Max(Cells.MinBound.y, point.CellMin.y - searchSize);
                int endY = Mathf.Min(point.CellMax.y + searchSize, Cells.MaxBound.y - 1);
            
                float sqrtRad = Mathf.Pow(point.FullRadius, 2);
                for (int y = startY; y <= endY; y++)
                {
                    float cellY = GridProperties.CellSize * y + GridProperties.PositionOffset.y;
                    for (int x = startX; x <= endX; x++)
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
        
        private bool IsInsideCircle(TPoint point, float sqrtRad, float x, float y)
        {
            double dx = x - point.WorldPosition.x;
            double dy = y - point.WorldPosition.y;
            double distanceSquared = dx * dx + dy * dy;
            return distanceSquared < sqrtRad;
        }
        
        private Candidate CreateCandidateFrom(PointGrid point, float candidateRadius)
        {
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: point.FullRadius,
                    candidateRadius: candidateRadius + PointProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = candidateRadius,
                Margin = PointProperties.PointMargin
            };
        }
        
        private bool IsCandidateValid(Candidate candidate)
        {
            int searchSize = GetSearchSize(candidate.Radius);
            
            int startX = Mathf.Max(Cells.MinBound.x, candidate.CellMin.x - searchSize);
            int endX = Mathf.Min(candidate.CellMax.x + searchSize, Cells.MaxBound.x - 1);
            int startY = Mathf.Max(Cells.MinBound.y, candidate.CellMin.y - searchSize);
            int endY = Mathf.Min(candidate.CellMax.y + searchSize, Cells.MaxBound.y - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    int pointIndex = Cells.GetCellValue(x, y);
                    if (pointIndex == 0) continue;

                    PointGrid existingPoint = GetPointByIndex(pointIndex);
                    if (candidate.IsIntersectWithPoint(existingPoint))
                    {
                        return false;
                    } 
                }
            }

            return true;
        }
        
        private int GetSearchSize(float pointRadius) => 
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridProperties.CellSize));
    }
}