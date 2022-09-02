using System.Collections.Generic;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public abstract class GridSingle
    {
        public GridCore GridProperties { get; }
        public int Tries { get; set; }
        public float Margin { get; set; }
        
        private readonly List<Point> _points;

        public GridSingle(GridCore gridProperties)
        {
            GridProperties = gridProperties;
            
            _points = new List<Point>();
        }

        public int TrySpawnPoint(Vector3 spawnerPosition, float spawnerRadius)
        {
            for (var i = 0; i < Tries; i++)
            {
                if (TryCreateCandidate(spawnerPosition, spawnerRadius, i, Tries, out var candidate))
                {
                    if (IsCandidateValid(candidate))
                    {
                        var point = new Point
                        {
                            WorldPosition = candidate.WorldPosition,
                            Radius = candidate.Radius - Margin,
                            Cell = candidate.Cell
                        };
                        _points.Add(point);
                        
                        GridProperties.SetCellValue(point.Cell.x, point.Cell.y, _points.Count);
                        PostPointCreated(point, _points.Count - 1);
                        return _points.Count - 1;
                    }
                }
                
                // if (GridProperties.IsPointInAABB(candidate.WorldPosition))
                // {
                //     candidate.Cell = GridProperties.PositionToCellClamped()
                // }
            }
            
            return -1;
        }
        
        protected abstract bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries, out Point candidate);

        protected abstract int GetSearchRange(float pointRadius);

        protected virtual void PostPointCreated(in Point point, int pointIndex)
        {
        }
        
        private bool IsCandidateValid(Point candidate)
        {
            var searchRange = GetSearchRange(candidate.Radius);
            var regionCoordinates = GridProperties.LookupRegionClamped(candidate.Cell, searchRange);
            
            var prevHorizontal = -1;

            for (var y = regionCoordinates.StartY; y < regionCoordinates.EndY; y++)
            {
                var coordStart = GridProperties.FlatCoordinates(regionCoordinates.StartX, y);
                var coordEnd =  GridProperties.FlatCoordinates(regionCoordinates.EndX, y);
                for (var x = coordStart; x < coordEnd; x++)
                {
                    var pointIndex = GridProperties.GetCellValue(x);
                    if(GridProperties.IsCellEmpty(pointIndex) || pointIndex == prevHorizontal) continue;
                    prevHorizontal = pointIndex;
                    
                    var existingPoint = _points[pointIndex - 1];
                    if (candidate.IsIntersectWithPoint(existingPoint))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}