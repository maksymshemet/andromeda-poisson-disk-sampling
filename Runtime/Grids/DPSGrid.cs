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
    public class DPSGrid<TPoint> : IDPSGrid<TPoint> where TPoint : DPSPoint, new()
    {
        public event Action<TPoint> OnPointCreated;
        public event Action<TPoint> OnPointRemoved;
        
        public ICellHolder Cells { get; }
        public GridProperties GridProperties { get; }
        public IEnumerable<TPoint> Points => GetPointsIEnumerable();
        public int PointsCount => _points.Count;

        private Queue<int> _emptyPointIndexes;
        
        private readonly IRadiusSelector _radiusSelector;
        private readonly ICandidateValidator _candidateValidator;
        private readonly List<TPoint> _points;

        public DPSGrid(ICellHolder cells, ICandidateValidator candidateValidator, GridProperties gridProperties, IRadiusSelector radiusSelector)
        {
            Cells = cells;
            GridProperties = gridProperties;
            _candidateValidator = candidateValidator;
            _radiusSelector = radiusSelector;

            _points = new List<TPoint>();
        }
        
        public TPoint GetPoint(int x, int y)
        {
            int index = Cells.GetCellValue(x, y) - 1;
            return index > -1 ? _points[index] : default;
        }
        
        public TPoint GetPointByIndex(int pointIndex) => _points[pointIndex - 1];

        public void RemovePoint(TPoint point)
        {
            int pointIndex = _points.IndexOf(point);
            if(pointIndex < 0) return;
            
            _points[pointIndex] = null;
            
            int pointCellIndex = pointIndex + 1;

            if (GridProperties.FillCellsInsidePoint)
            {
                int searchSize = GetSearchSize(point.Size);

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
            
            OnPointRemoved?.Invoke(point);
        }
        
        public TPoint TrySpawnPointFrom(TPoint point)
        {
            for (var i = 0; i < GridProperties.Tries; i++)
            {
                PointSize candidateSize = _radiusSelector.GetRadius(i, GridProperties.Tries);
                Candidate candidate = CreateCandidateFrom(point, candidateSize);
                
                TPoint newPoint = TryAddPoint(candidate);
                if (newPoint != null)
                {
                    return newPoint;
                }
            }
        
            return null;
        }

        public TPoint TryAddPoint(Candidate candidate)
        {
            if (IsCandidateInAABB(candidate))
            {
                int searchSize = GetSearchSize(candidate.Size);
                
                if (_candidateValidator.IsValid(this, candidate, searchSize))
                {
                    var newPoint = new TPoint
                    {
                        WorldPosition = candidate.WorldPosition,
                        Size = candidate.Size,
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

        public HashSet<TPoint> GetPointsAround(in TPoint point, int region)
        {
            HashSet<TPoint> set = GetPointsAround(point.CellMin, point.CellMax, region);
            set.Remove(point);
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
            Cells.Clear();
            
            _points.Clear();
            _emptyPointIndexes?.Clear();
        }

        public bool IsPositionFree(Candidate candidate, int x, int y)
        {
            int pointIndex = Cells.GetCellValue(x, y);
            if (pointIndex == 0) return false;
            
            TPoint existingPoint = GetPointByIndex(pointIndex);
            
            float sqrDst = (existingPoint.WorldPosition - candidate.WorldPosition).sqrMagnitude;
            float radius = existingPoint.Size.Radius + existingPoint.Size.Margin 
                                                + candidate.Size.Margin + candidate.Size.Radius 
                                                + GridProperties.PointMargin + GridProperties.PointMargin;
            return sqrDst < (radius * radius);
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
                           candidate.WorldPosition.x - candidate.Size.Radius,
                           candidate.WorldPosition.y - candidate.Size.Radius)) &&
                       Cells.IsPositionInAABB(new Vector3(
                           candidate.WorldPosition.x + candidate.Size.Radius,
                           candidate.WorldPosition.y + candidate.Size.Radius));
            }
            
            float fullRadius = candidate.Size.Radius + candidate.Size.Margin + GridProperties.PointMargin;
            return Cells.IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x - fullRadius,
                       candidate.WorldPosition.y - fullRadius)) &&
                   Cells.IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x + fullRadius,
                       candidate.WorldPosition.y + fullRadius));
        }
        
        public virtual void StorePoint(TPoint point)
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
            
            OnPointCreated?.Invoke(point);
        }
        
        private void FillGridValues(in TPoint point, int pointCellIndex)
        {
            if (GridProperties.FillCellsInsidePoint)
            {
                float fullRadius = point.Size.Radius + point.Size.Margin + GridProperties.PointMargin;
                int searchSize = Mathf.RoundToInt(fullRadius / GridProperties.CellSize);
                SearchBoundaries searchBoundaries = Helper.GetSearchBoundaries(this, point.CellMin, point.CellMax, searchSize);
            
                float sqrtRad = fullRadius * fullRadius;
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
        
        private bool IsInsideCircle(TPoint point, float sqrtRad, float x, float y)
        {
            double dx = x - point.WorldPosition.x;
            double dy = y - point.WorldPosition.y;
            double distanceSquared = dx * dx + dy * dy;
            return distanceSquared < sqrtRad;
        }
        
        private Candidate CreateCandidateFrom(TPoint point, PointSize candidateSize)
        {
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: point.Size.Radius + point.Size.Margin + GridProperties.PointMargin,
                    candidateRadius: candidateSize.Radius + candidateSize.Margin + GridProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Size = candidateSize
            };
        }
        
        private int GetSearchSize(PointSize size) => 
            Mathf.Max(3, Mathf.CeilToInt((size.Margin + size.Radius + GridProperties.PointMargin) / GridProperties.CellSize));

        private IEnumerable<TPoint> GetPointsIEnumerable()
        {
            foreach (TPoint point in _points)
            {
                if(point != null)
                    yield return point;
            }
        }
    }


    public class DPSGrid : DPSGrid<DPSPoint>, IDPSGrid
    {
        public DPSGrid(ICellHolder cells, ICandidateValidator candidateValidator, GridProperties gridProperties, IRadiusSelector radiusSelector) : base(cells, candidateValidator, gridProperties, radiusSelector)
        {
        }
    }
}