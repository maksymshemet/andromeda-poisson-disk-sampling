using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public abstract class GridWorld : IFiller
    {
        public GridCore GridCore { get; }
        public WorldMultiRad World { get; }
        public Vector2Int ChunkPosition { get; }

        public IRadius Radius => World.Radius;
        public int Tries => World.Tries;

        private readonly List<PointWorld> _points;
        private Stack<int> _emptyPointIndices;
        private Dictionary<int, PointWorld> _linkedPoints; 

        protected GridWorld(GridCore gridCore,WorldMultiRad world, Vector2Int chunkPosition)
        {
            GridCore = gridCore;
            World = world;
            ChunkPosition = chunkPosition;
            _points = new List<PointWorld>();
        }

        public void LinkPoint(int x, int y, in PointWorld point)
        {
            if (_linkedPoints == null)
                _linkedPoints = new Dictionary<int, PointWorld>();

            var key = GridCore.FlatCoordinates(x, y);
            
            _linkedPoints[key] = point;
        }

        public void RemoveLink(int x, int y, in PointWorld point)
        {
            if(_linkedPoints == null) return;
        
            var key = GridCore.FlatCoordinates(x, y);
            if (_linkedPoints.TryGetValue(key, out var linkedPoint))
            {
                if (linkedPoint == point)
                {
                    _linkedPoints.Remove(key);
                }
            }
        }

        public IReadOnlyList<PointWorld> GetPoints()
        {
            return _points;
        }

        public PointWorld GetPoint(int x, int y, bool excludeLinked = false)
        {
            if (GridCore.IsCellEmpty(x, y))
            {
                if (!excludeLinked && _linkedPoints != null)
                {
                    if (_linkedPoints.TryGetValue(GridCore.FlatCoordinates(x, y), out var lp))
                        return lp;
                }
                return null;
            }
                
            
            var pointIndex = GridCore.GetCellValue(x, y) - 1;
            return _points[pointIndex];
        }

        public virtual bool RemovePoint(PointWorld point)
        {
            if (point.ChunkPosition != ChunkPosition && GridCore.IsCellEmpty(point.Cell.x, point.Cell.y))
            {
                return false;
            }

            var pointIndex = GridCore.GetCellValue(point.Cell.x, point.Cell.y) - 1;
            if (_points[pointIndex] != null)
            {
                if (_emptyPointIndices == null)
                    _emptyPointIndices = new Stack<int>();
                    
                _emptyPointIndices.Push(pointIndex);
            }
                
            _points[pointIndex] = null;

            GridCore.ClearCell(point.Cell.x, point.Cell.y);
            
            return true;
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, out PointWorld candidate)
        { 
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(0, Tries), 0, Tries, out candidate);
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, out PointWorld candidate)
        {
            return TryCreateCandidate(spawnerPosition, spawnerRadius, 0, Tries, out candidate);
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, int currentTry, int maxTries, out PointWorld candidate)
        {
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(currentTry, maxTries), currentTry, maxTries, out candidate);
        }
        
        public abstract bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries, out PointWorld candidate);

        public virtual bool TryAddPoint(PointWorld point)
        {
            if (GridCore.IsCellEmpty(point.Cell.x, point.Cell.y) && IsCandidateValid(point))
            {
                point.ChunkPosition = ChunkPosition;
                point.Radius -= World.Margin;
                
                if (_emptyPointIndices?.Count > 0)
                {
                    var index = _emptyPointIndices.Pop();
                    _points[index] = point;
                    GridCore.SetCellValue(point.Cell.x, point.Cell.y, index + 1);
                }
                else
                {
                    _points.Add(point);
                    GridCore.SetCellValue(point.Cell.x, point.Cell.y, _points.Count);
                }
                return true;
            }
            return false;
        }

        protected abstract int GetSearchRange(float pointRadius);
        
        private bool IsCandidateValid(PointWorld candidate)
        {
            var searchRange = GetSearchRange(candidate.Radius);
            var regionCoordinates = GridCore.LookupRegion(candidate.Cell, searchRange);

            if (GridCore.IsRegionInsideGrid(regionCoordinates))
            {
                for (var y = regionCoordinates.StartY; y < regionCoordinates.EndY; y++)
                {
                    var coordStart = GridCore.FlatCoordinates(regionCoordinates.StartX, y);
                    var coordEnd =  GridCore.FlatCoordinates(regionCoordinates.EndX, y);
                    for (var x = coordStart; x <= coordEnd; x++)
                    {
                        if (!IsCandidateValidInGrid(x, candidate))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
                {
                    for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                    {
                        if (GridCore.IsCellInGrid(x, y))
                        {
                            var index = GridCore.FlatCoordinates(x, y);
                            if (!IsCandidateValidInGrid(index, candidate))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!IsCandidateValidInWorld(x, y, candidate))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            
            
            return true;
        }

        private bool IsCandidateValidInGrid(int index, PointWorld candidate)
        {
            if (GridCore.IsCellEmpty(index))
            {
                if (_linkedPoints != null)
                {
                    if (_linkedPoints.TryGetValue(index, out var linkedPoint))
                    {
                        return !candidate.IsIntersectWithPoint(linkedPoint);
                    }
                }
                return true;
            }
            
            var pointIndex = GridCore.GetCellValue(index);
            var point = _points[pointIndex - 1]; 
            return !candidate.IsIntersectWithPoint(point);
        }
        
        private bool IsCandidateValidInWorld(int x, int y, PointWorld candidate)
        {
            var realWorld = World.GetRealWorldCoordinate(ChunkPosition, x, y);
            var point = World.GetPoint(realWorld);
            if(point == null)
            {
                return true;
            }

            return !candidate.IsIntersectWithPoint(point);
        }
    }
}