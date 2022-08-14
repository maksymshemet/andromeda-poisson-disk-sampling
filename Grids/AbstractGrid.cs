using System;
using System.Collections;
using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class AbstractGrid : IGrid
    {
        private const float DoublePI = 6.28318548f;
        
        public GridProperties GridProperties { get; }
        public Vector3 WorldPositionOffset { get; set; }
        public ICandidateValidator CandidateValidator { get; set; }
        
        public IReadOnlyList<Point> Points => _points;

        private readonly int[] _cells;
        protected readonly List<Point> _points;

        protected AbstractGrid(GridProperties gridProperties)
        {
            GridProperties = gridProperties;
            WorldPositionOffset = Vector3.zero;
            
            _cells = new int[gridProperties.CellWidth * gridProperties.CellHeight];
            _points = new List<Point>();
        }

        public List<Point> Fill() => 
            Fill(new Vector3(GridProperties.Size.x / 2, GridProperties.Size.y / 2) + WorldPositionOffset);
        
        public virtual List<Point> Fill(Vector3 startSpawnPoint)
        {
            var pointIndex = TrySpawnPoint(startSpawnPoint, GetPointRadius(0));
            if (pointIndex <= -1)
                throw new Exception("Couldn't spawn the point");
            
            var spawnPoints = new List<int> { pointIndex };

            do {
                var spawnIndex = Random.Range(0, spawnPoints.Count);
                var spawnPointIndex = spawnPoints[spawnIndex];
                var spawnPoint = _points[spawnPointIndex];

                pointIndex = TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius);
                
                if (pointIndex > -1)
                {
                    spawnPoints.Add(pointIndex);
                }
                else
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }

#if UNITY_EDITOR
                if (EditorCheckForEndlessSpawn(spawnPoints)) break;
#endif
            }
            while (spawnPoints.Count > 0);
            
            return _points;
        }
        
        public bool TrySpawnPointAt(Vector3 startPosition) => 
            TrySpawnPoint(startPosition, GetPointRadius(0)) > -1;
        
        public bool TrySpawnPointAt(Vector3 startPosition, out Point point)
        {
            var pointIndex= TrySpawnPoint(startPosition, GetPointRadius(0));
            if (pointIndex > -1)
            {
                point = _points[pointIndex];
                return true;
            }

            point = default;
            return false;
        }
        
        public bool TrySpawnPointFrom(Point spawnPoint) => 
            TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius) > -1;
        
        public bool TrySpawnPointFrom(Point spawnPoint, out Point point)
        {
            var pointIndex= TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius);
            if (pointIndex > -1)
            {
                point = _points[pointIndex];
                return true;
            }

            point = default;
            return false;
        }

        public int GetCellValue(int index) =>
            _cells[index];
        
        public int GetCellValue(Vector2Int coordinates) =>
            GetCellValue(coordinates.x, coordinates.y);

        public int GetCellValue(int x, int y) => 
            _cells[FlatCoordinates(x, y)];
        
        public void SetCellValue(int x, int y, int value) => 
            _cells[FlatCoordinates(x, y)] = value;

       public void SetCellValue(int index, int value) =>
            _cells[index] = value;
        
        public Point GetPointValue(int x, int y)
        {
            var cellValue = GetCellValue(x, y);
            if (cellValue == 0)
                return null;
            
            return _points[cellValue - 1];
        }

        public Point GetPointValue(int index) =>
            _points[index];
        
        public int AddPoint(int x, int y, Point point)
        {
            _points.Add(point);
            var pointIndex = _points.Count;
            SetCellValue(x, y, pointIndex);
            return pointIndex - 1;
        }

        
        protected virtual int TrySpawnPoint(Vector3 spawnerPosition, float spawnerRadius)
        {
            for (var i = 0; i < GridProperties.Tries; i++)
            {
                var candidateRadius = GetPointRadius(i);
                if (candidateRadius <= 0) continue;
                
                var candidateWorldPosition = GetRandomCandidateWorldPosition(
                    spawnerPosition, 
                    spawnerRadius, 
                    candidateRadius);
                
                if (IsPointInRegionBoundaries(candidateWorldPosition))
                {
                    var candidateCell = CellFromPoint(candidateWorldPosition);
                    var searchSize = GetSearchSize(candidateRadius);
                    if (IsCandidateValid(searchSize, candidateWorldPosition, candidateRadius, candidateCell))
                    {
                        var point = CreatePoint(candidateWorldPosition, candidateRadius, candidateCell);
                        // new Point
                        // (
                        //     pointIndex: _points.Count,
                        //     worldPosition: candidateWorldPosition,
                        //     cell: candidateCell,
                        //     radius: candidateRadius
                        // );

                        _points.Add(point);
                        _cells[FlatCoordinates(point.Cell)] = _points.Count;
                        OnPointCreated(ref point);
                        return point.PointIndex;
                    }
                }
            }
            
            return -1;
        }

        protected virtual Point CreatePoint(Vector3 candidateWorldPosition, float candidateRadius, Vector2Int candidateCell)
        {
            return new Point
            (
                pointIndex: _points.Count,
                worldPosition: candidateWorldPosition,
                cell: candidateCell,
                radius: candidateRadius
            );
        }
        
        protected virtual bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
            Vector2Int candidateCell) =>
            CandidateValidator.IsCandidateValid(this, searchSize, candidateWorldPosition, candidateRadius,
                candidateCell);
        
        protected virtual void OnPointCreated(ref Point point)
        {
        }
        
        protected abstract float GetPointRadius(int currentTry);
        
        public int FlatCoordinates(int x, int y) => y * GridProperties.CellWidth + x;
        
        protected int FlatCoordinates(Vector2Int coord) => FlatCoordinates(coord.x, coord.y);
        
        protected Vector3 GetRandomCandidateWorldPosition(Vector3 spawnPointWorldPosition, 
            float spawnerRadius, float candidateRadius)
        {
            var angel = Random.value * DoublePI;
            var direction = new Vector3(Mathf.Sin(angel), Mathf.Cos(angel));
            return spawnPointWorldPosition + direction * Random.Range(2 * spawnerRadius, candidateRadius * 3f);
        }
        
        protected Vector2Int CellFromPoint(Vector3 point)
        {
            var x = Mathf.RoundToInt((point.x - WorldPositionOffset.x) / GridProperties.CellSize);
            var y = Mathf.RoundToInt((point.y - WorldPositionOffset.y) / GridProperties.CellSize);
            return new Vector2Int(
                x: Mathf.Max(0,Mathf.Min(x, GridProperties.CellWidth - 1)),
                y: Mathf.Max(0,Mathf.Min(y, GridProperties.CellHeight - 1)));
        }
        
        protected bool IsPointInRegionBoundaries(Vector3 candidatePoint)
        {
            return candidatePoint.x >= WorldPositionOffset.x &&
                   candidatePoint.y >= WorldPositionOffset.y &&
                   candidatePoint.x <= WorldPositionOffset.x + GridProperties.Size.x &&
                   candidatePoint.y <= WorldPositionOffset.y + GridProperties.Size.y;
        }

        protected abstract int GetSearchSize(float pointRadius);
        
#if UNITY_EDITOR
        private bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
        {
            if (GridProperties.CellWidth * GridProperties.CellHeight >= _points.Count) return false;
            
            Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
            return true;
        }
#endif
    }
    
//     public abstract class AbstractGrid2<T> : IGridAbstract<T> where T : Point
//     {
//         private const float DoublePI = 6.28318548f;
//         
//         public GridProperties GridProperties { get; }
//         public Vector3 WorldPositionOffset { get; set; }
//         public ICandidateValidator CandidateValidator { get; set; }
//         
//         public IReadOnlyList<T> Points => _points;
//
//         private readonly int[] _cells;
//         protected readonly List<T> _points;
//
//         protected AbstractGrid2(GridProperties gridProperties)
//         {
//             GridProperties = gridProperties;
//             WorldPositionOffset = Vector3.zero;
//             
//             _cells = new int[gridProperties.CellWidth * gridProperties.CellHeight];
//             _points = new List<T>();
//         }
//
//         public List<T> Fill() => 
//             Fill(new Vector3(GridProperties.Size.x / 2, GridProperties.Size.y / 2) + WorldPositionOffset);
//         
//         public virtual List<T> Fill(Vector3 startSpawnPoint)
//         {
//             var pointIndex = TrySpawnPoint(startSpawnPoint, GetPointRadius(0));
//             if (pointIndex <= -1)
//                 throw new Exception("Couldn't spawn the point");
//             
//             var spawnPoints = new List<int> { pointIndex };
//
//             do {
//                 var spawnIndex = Random.Range(0, spawnPoints.Count);
//                 var spawnPointIndex = spawnPoints[spawnIndex];
//                 var spawnPoint = _points[spawnPointIndex];
//
//                 pointIndex = TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius);
//                 
//                 if (pointIndex > -1)
//                 {
//                     spawnPoints.Add(pointIndex);
//                 }
//                 else
//                 {
//                     spawnPoints.RemoveAt(spawnIndex);
//                 }
//
// #if UNITY_EDITOR
//                 if (EditorCheckForEndlessSpawn(spawnPoints)) break;
// #endif
//             }
//             while (spawnPoints.Count > 0);
//             
//             return _points;
//         }
//         
//         public bool TrySpawnPointAt(Vector3 startPosition) => 
//             TrySpawnPoint(startPosition, GetPointRadius(0)) > -1;
//         
//         public bool TrySpawnPointAt(Vector3 startPosition, out Point point)
//         {
//             var pointIndex= TrySpawnPoint(startPosition, GetPointRadius(0));
//             if (pointIndex > -1)
//             {
//                 point = _points[pointIndex];
//                 return true;
//             }
//
//             point = default;
//             return false;
//         }
//         
//         public bool TrySpawnPointFrom(Point spawnPoint) => 
//             TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius) > -1;
//         
//         public bool TrySpawnPointFrom(Point spawnPoint, out Point point)
//         {
//             var pointIndex= TrySpawnPoint(spawnPoint.WorldPosition, spawnPoint.Radius);
//             if (pointIndex > -1)
//             {
//                 point = _points[pointIndex];
//                 return true;
//             }
//
//             point = default;
//             return false;
//         }
//
//         public int GetCellValue(int index) =>
//             _cells[index];
//         
//         public int GetCellValue(Vector2Int coordinates) =>
//             GetCellValue(coordinates.x, coordinates.y);
//
//         public int GetCellValue(int x, int y) => 
//             _cells[FlatCoordinates(x, y)];
//         
//         public void SetCellValue(int x, int y, int value) => 
//             _cells[FlatCoordinates(x, y)] = value;
//
//        public void SetCellValue(int index, int value) =>
//             _cells[index] = value;
//         
//         public T GetPointValue(int x, int y)
//         {
//             var cellValue = GetCellValue(x, y);
//             if (cellValue == 0)
//                 return default;
//             
//             return _points[cellValue - 1];
//         }
//
//         public T GetPointValue(int index) =>
//             _points[index];
//         
//         public int AddPoint(int x, int y, T point)
//         {
//             _points.Add(point);
//             var pointIndex = _points.Count;
//             SetCellValue(x, y, pointIndex);
//             return pointIndex - 1;
//         }
//
//         
//         protected virtual int TrySpawnPoint(Vector3 spawnerPosition, float spawnerRadius)
//         {
//             for (var i = 0; i < GridProperties.Tries; i++)
//             {
//                 var candidateRadius = GetPointRadius(i);
//                 if (candidateRadius <= 0) continue;
//                 
//                 var candidateWorldPosition = GetRandomCandidateWorldPosition(
//                     spawnerPosition, 
//                     spawnerRadius, 
//                     candidateRadius);
//                 
//                 if (IsPointInRegionBoundaries(candidateWorldPosition))
//                 {
//                     var candidateCell = CellFromPoint(candidateWorldPosition);
//                     var searchSize = GetSearchSize(candidateRadius);
//                     if (IsCandidateValid(searchSize, candidateWorldPosition, candidateRadius, candidateCell))
//                     {
//                         var point = CreatePoint(candidateWorldPosition, candidateRadius, candidateCell);
//                         // new Point
//                         // (
//                         //     pointIndex: _points.Count,
//                         //     worldPosition: candidateWorldPosition,
//                         //     cell: candidateCell,
//                         //     radius: candidateRadius
//                         // );
//
//                         _points.Add(point);
//                         _cells[FlatCoordinates(point.Cell)] = _points.Count;
//                         OnPointCreated(ref point);
//                         return point.PointIndex;
//                     }
//                 }
//             }
//             
//             return -1;
//         }
//
//         protected abstract T CreatePoint(Vector3 candidateWorldPosition, float candidateRadius,
//             Vector2Int candidateCell);
//         
//         protected virtual bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
//             Vector2Int candidateCell) =>
//             CandidateValidator.IsCandidateValid(this, searchSize, candidateWorldPosition, candidateRadius,
//                 candidateCell);
//         
//         protected virtual void OnPointCreated(ref T point)
//         {
//         }
//         
//         protected abstract float GetPointRadius(int currentTry);
//         
//         public int FlatCoordinates(int x, int y) => y * GridProperties.CellWidth + x;
//         
//         protected int FlatCoordinates(Vector2Int coord) => FlatCoordinates(coord.x, coord.y);
//         
//         protected Vector3 GetRandomCandidateWorldPosition(Vector3 spawnPointWorldPosition, 
//             float spawnerRadius, float candidateRadius)
//         {
//             var angel = Random.value * DoublePI;
//             var direction = new Vector3(Mathf.Sin(angel), Mathf.Cos(angel));
//             return spawnPointWorldPosition + direction * Random.Range(2 * spawnerRadius, candidateRadius * 3f);
//         }
//         
//         protected Vector2Int CellFromPoint(Vector3 point)
//         {
//             var x = Mathf.RoundToInt((point.x - WorldPositionOffset.x) / GridProperties.CellSize);
//             var y = Mathf.RoundToInt((point.y - WorldPositionOffset.y) / GridProperties.CellSize);
//             return new Vector2Int(
//                 x: Mathf.Max(0,Mathf.Min(x, GridProperties.CellWidth - 1)),
//                 y: Mathf.Max(0,Mathf.Min(y, GridProperties.CellHeight - 1)));
//         }
//         
//         protected bool IsPointInRegionBoundaries(Vector3 candidatePoint)
//         {
//             return candidatePoint.x >= WorldPositionOffset.x &&
//                    candidatePoint.y >= WorldPositionOffset.y &&
//                    candidatePoint.x <= WorldPositionOffset.x + GridProperties.Size.x &&
//                    candidatePoint.y <= WorldPositionOffset.y + GridProperties.Size.y;
//         }
//
//         protected abstract int GetSearchSize(float pointRadius);
//         
// #if UNITY_EDITOR
//         private bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
//         {
//             if (GridProperties.CellWidth * GridProperties.CellHeight >= _points.Count) return false;
//             
//             Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
//             return true;
//         }
// #endif
//    }
}