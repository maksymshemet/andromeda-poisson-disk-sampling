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
                if (_linkedPoints != null)
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
        
        public bool TrySpawnPointNear(Candidate candidate, out PointWorld point)
        {
            if (IsCandidateValid(candidate))
            {
                return TryAddPoint(candidate.WorldPosition, 
                    candidate.Radius, candidate.Cell.x, candidate.Cell.y, out point, true);
            }
            
            point = default;
            return false;
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, out Candidate candidate)
        {
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(0, Tries), 0, Tries, out candidate);
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, out Candidate candidate)
        {
            return TryCreateCandidate(spawnerPosition, spawnerRadius, 0, Tries, out candidate);
        }
        
        public bool TryCreateCandidate(Vector3 spawnerPosition, int currentTry, int maxTries, out Candidate candidate)
        {
            return TryCreateCandidate(spawnerPosition, Radius.GetRadius(currentTry, maxTries), currentTry, maxTries, out candidate);
        }
        
        public abstract bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries, out Candidate candidate);

        protected virtual bool TryAddPoint(Vector3 worldPosition, float radius, int x, int y, out PointWorld point, bool force = false)
        {
            if (force || GridCore.IsCellEmpty(x, y))
            {
                point = new PointWorld
                {
                    WorldPosition = worldPosition,
                    Radius = radius - World.Margin,
                    Cell = new Vector2Int(x, y),
                    ChunkPosition = ChunkPosition
                };

                if (_emptyPointIndices?.Count > 0)
                {
                    var index = _emptyPointIndices.Pop();
                    _points[index] = point;
                    GridCore.SetCellValue(x, y, index + 1);
                }
                else
                {
                    _points.Add(point);
                    GridCore.SetCellValue(x, y, _points.Count);
                }
                return true;
            }

            point = default;
            return false;
        }

        protected abstract int GetSearchRange(float pointRadius);
        
        private bool IsCandidateValid(Candidate candidate)
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

        private bool IsCandidateValidInGrid(int x, int y, Candidate candidate)
        {
            if (GridCore.IsCellEmpty(x, y))
            {
                if(_linkedPoints == null)
                    return true;

                var key = new Vector2Int(x, y);
                if (_linkedPoints.TryGetValue(key, out var linkedPoint))
                {
                    return !IsCandidateIntersectWithPoint(candidate, linkedPoint);
                }
                
                return true;
            }
            
            var pointIndex = GridCore.GetCellValue(x, y);
            var point = _points[pointIndex - 1]; 
            return !IsCandidateIntersectWithPoint(candidate, point);
        }
        
        private bool IsCandidateValidInWorld(int x, int y, Candidate candidate)
        {
            var realWorld = World.GetRealWorldCoordinate(ChunkPosition, x, y);
            var point = World.GetPoint(realWorld);
            if(point == null)
            {
                return true;
            }

            return !IsCandidateIntersectWithPoint(candidate, point);
        }
        
        private bool IsCandidateIntersectWithPoint(Candidate candidate, Point point)
        {
            var sqrDst = (point.WorldPosition - candidate.WorldPosition).sqrMagnitude;
            var radius = point.Radius + candidate.Radius;
            return sqrDst < (radius * radius);
        }
    }
}