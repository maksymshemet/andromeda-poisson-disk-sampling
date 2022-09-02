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
        private Dictionary<Vector2Int, PointWorld> _linkedPoints; 

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
                _linkedPoints = new Dictionary<Vector2Int, PointWorld>();
            
            var key = new Vector2Int(x, y);
            
            _linkedPoints[key] = point;
        }

        public void RemoveLink(int x, int y, in PointWorld point)
        {
            if(_linkedPoints == null) return;
        
            var key = new Vector2Int(x, y);
            if (_linkedPoints.TryGetValue(key, out var linkedPoint))
            {
                if (linkedPoint == point)
                {
                    _linkedPoints.Remove(key);
                }
            }
        }

        public List<PointWorld> GetPoints()
        {
            return _points;
        }

        public PointWorld GetPoint(int x, int y, bool excludeLinked = false)
        {
            if (GridCore.IsCellEmpty(x, y))
            {
                if (!excludeLinked && _linkedPoints != null)
                {
                    if (_linkedPoints.TryGetValue(new Vector2Int(x, y), out var lp))
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
        
        public bool TrySpawnPointNear(PointWorld candidate)
        {
            if (GridCore.IsCellEmpty(candidate.Cell.x, candidate.Cell.y) && IsCandidateValid(candidate))
            {
                return TryAddPoint(candidate);
            }
            
            return false;
        }
        
        public PointWorld TryCreateCandidate(Vector3 spawnerPosition)
        { 
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(0, Tries), 0, Tries);
        }
        
        public PointWorld TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius)
        {
            return TryCreateCandidate(spawnerPosition, spawnerRadius, 0, Tries);
        }
        
        public PointWorld TryCreateCandidate(Vector3 spawnerPosition, int currentTry, int maxTries)
        {
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(currentTry, maxTries), currentTry, maxTries);
        }
        
        public abstract PointWorld TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries);

        protected virtual bool TryAddPoint(PointWorld point)
        {
            if (GridCore.IsCellEmpty(point.Cell.x, point.Cell.y))
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

            for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
            {
                for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                {
                    var r = GridCore.IsCellInGrid(x, y)
                        ? IsCandidateValidInGrid(x, y, candidate)
                        : IsCandidateValidInWorld(x, y, candidate);

                    if (!r) return false;
                }
            }
            
            return true;
        }

        private bool IsCandidateValidInGrid(int x, int y, PointWorld candidate)
        {
            if (GridCore.IsCellEmpty(x, y))
            {
                if (_linkedPoints != null)
                {
                    var key = new Vector2Int(x, y);
                    if (_linkedPoints.TryGetValue(key, out var linkedPoint))
                    {
                        return !candidate.IsIntersectWithPoint(linkedPoint);
                    }
                }
                
                return true;
            }
            
            var pointIndex = GridCore.GetCellValue(x, y);
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