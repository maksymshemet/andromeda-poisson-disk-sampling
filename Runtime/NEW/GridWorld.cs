using System;
using System.Collections;
using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public abstract class GridWorld
    {
        public GridCore GridCore { get; }
        public WorldMultiRad World { get; }
        public Vector2Int ChunkPosition { get; }
        public int Tries { get; set; }
        public float Margin { get; set; }
        
        protected readonly IRadius radiusProvider;
        
        private readonly List<PointWorld> _points;
        private Stack<int> _emptyPointIndices;
        
        protected GridWorld(GridCore gridCore,WorldMultiRad world, Vector2Int chunkPosition, IRadius radius)
        {
            radiusProvider = radius;
            GridCore = gridCore;
            World = world;
            ChunkPosition = chunkPosition;

            _points = new List<PointWorld>();
        }

        public List<PointWorld> Fill()
        {
           return Fill(new Vector3(GridCore.Size.x / 2, GridCore.Size.y / 2) + GridCore.WorldPositionOffset);
        }
        
        public List<PointWorld> Fill(Vector3 spawnPosition)
        {
            PointWorld pointIndex;
            if(!TrySpawnPoint(spawnPosition, radiusProvider.GetRadius(0, Tries), out pointIndex))
            {
                throw new Exception("Couldn't spawn the point");
            }
            
            var spawnPoints = new List<PointWorld> { pointIndex };

            do {
               

                try
                {
                    var spawnIndex = Random.Range(0, spawnPoints.Count);
                    var spawnPoint = spawnPoints[spawnIndex];
                    // var spawnPoint = _points[spawnPointIndex];

                    if (TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius, out pointIndex))
                    {
                        spawnPoints.Add(pointIndex);
                    }
                    else
                    {
                        spawnPoints.RemoveAt(spawnIndex);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

#if UNITY_EDITOR
                if (EditorCheckForEndlessSpawn(spawnPoints)) break;
#endif
            }
            while (spawnPoints.Count > 0);
            
            return _points;
        }
        
        public PointWorld GetPoint(int x, int y)
        {
            if(GridCore.IsCellEmpty(x, y))
                return null;
            
            var pointIndex = GridCore.GetCellValue(x, y) - 1;
            return _points[pointIndex];
        }

        public bool TrySpawnPoint(Vector3 spawnerPosition, out PointWorld point)
        {
            return TrySpawnPoint(spawnerPosition, radiusProvider.GetRadius(0, Tries), out point);
        }
        
        public bool TrySpawnPoint(Vector3 spawnerPosition, float spawnerRadius, out PointWorld point)
        {
            for (var i = 0; i < Tries; i++)
            {
                if (TryCreateCandidate(spawnerPosition, spawnerRadius, i, Tries, out var candidate))
                {
                    if (IsCandidateValid(candidate))
                    {
                        TryAddPoint(candidate.WorldPosition, candidate.Radius, candidate.Cell.x, candidate.Cell.y, out point, true);
                        PostPointCreated(point, GridCore.GetCellValue(candidate.Cell.x, candidate.Cell.y) - 1);
                        return true;
                    }
                }
            }
            
            point = default;
            return false;
        }
        
        public bool TryAddPoint(Vector3 worldPosition, float radius, int x, int y, out PointWorld point, bool force = false)
        {
            if (force || GridCore.IsCellEmpty(x, y))
            {
                point = new PointWorld
                {
                    WorldPosition = worldPosition,
                    Radius = radius - Margin,
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

        public bool RemovePoint(PointWorld point)
        {
            if (point.ChunkPosition != ChunkPosition)
            {
                return false;
            }

            var cellValue = GridCore.GetCellValue(point.Cell.x, point.Cell.y);
            
            if(ClearCell(point.Cell.x, point.Cell.y, true))
            {
                OnPointRemoved(point, cellValue - 1, cellValue);
                return true;
            }

            return false;
        }

        public bool ClearCell(int cellX, int cellY, bool removePoint = true)
        {
            if (GridCore.IsCellEmpty(cellX, cellY))
            {
                return false;
            }
            
            if (removePoint)
            {
                var pointIndex = GridCore.GetCellValue(cellX, cellY) - 1;
                if (_points[pointIndex] != null)
                {
                    if (_emptyPointIndices == null)
                        _emptyPointIndices = new Stack<int>();
                    
                    _emptyPointIndices.Push(pointIndex);
                }
                
                _points[pointIndex] = null;
            }
            
            GridCore.CleatCell(cellX, cellY);
            
            return true;
        }
        
        protected virtual void OnPointRemoved(PointWorld point, int pointIndex,  int cellValue)
        {

        }
        
        protected abstract bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries, out Candidate candidate);

        protected abstract int GetSearchRange(float pointRadius);
        
        protected virtual void PostPointCreated(in PointWorld point, int pointIndex)
        {
        }

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
            if (GridCore.IsCellEmpty(x, y)) return true;
            
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
        
#if UNITY_EDITOR
        private bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
        {
            if (GridCore.CellWidth * GridCore.CellHeight >= _points.Count) return false;
            
            Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
            return true;
        }
#endif
    }
}