using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridMultiRad : Grid<PointPropertiesMultiRadius, GridMultiRad>
    {
        public GridMultiRad(PointPropertiesMultiRadius userProperties, GridProperties gridProperties) 
            : base(userProperties, gridProperties)
        {
        }
        
        public void Fill()
        {
            Vector3 fakeWorldPosition = new Vector3(
                x: GridProperties.Size.x / 2f, 
                y: GridProperties.Size.y / 2f) + GridProperties.PositionOffset;

            var fakePoint = new PointGrid
            {
                WorldPosition = fakeWorldPosition,
                Radius = UserProperties.RadiusProvider.GetRandomRadius(0, UserProperties.Tries),
                // Cell = WorldPositionToCell(fakeWorldPosition)
            };
            
            int pointIndex = TrySpawnPointFrom(fakePoint, out PointGrid _);
            if (pointIndex == -1)
            {
                throw new Exception("Couldn't spawn the point");
            }

            var spawnPoints = new List<int> { pointIndex };
            
            do
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                int spawnPointIndex = spawnPoints[spawnIndex];
                PointGrid spawnPoint = PointsInternal[spawnPointIndex];
                
                pointIndex = TrySpawnPointFrom(spawnPoint, out PointGrid _);
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
        }
        
        public int TrySpawnPointFrom(PointGrid point, out PointGrid newPoint)
        {
            for (var i = 0; i < UserProperties.Tries; i++)
            {
                Candidate candidate = CreateCandidateFrom(point, i);

                if (IsCandidateInAABB(candidate))
                {
                    Vector2Int candidateCell = WorldPositionToCell(candidate.WorldPosition);
                    if(!IsValidCellPosition(candidateCell))
                        continue;
                    
                    Vector2Int candidateCellMax = WorldPositionToCell(candidate.WorldPosition, 
                        WorldToCellPositionMethod.Ceil);
                    Vector2Int candidateCellMin = WorldPositionToCell(candidate.WorldPosition, 
                        WorldToCellPositionMethod.Floor);

                    if (IsCandidateValid(candidate, candidateCellMax, candidateCellMin))
                    {
                        newPoint = new PointGrid
                        {
                            WorldPosition = candidate.WorldPosition,
                            Radius = candidate.Radius,
                            Margin = candidate.Margin
                        };
                        AddPoint(newPoint);
                        return PointsInternal.Count - 1;
                    }
                }
            }

            newPoint = default;
            return -1;
        }
        
        protected override void OnPointCreatedInternal(in PointGrid point)
        {
            base.OnPointCreatedInternal(point);
            
            int searchSize = Mathf.RoundToInt((point.Radius + UserProperties.PointMargin) / GridProperties.CellSize);
            int startX = Mathf.Max(0, point.CellMin.x - searchSize);
            int endX = Mathf.Min(point.CellMax.x + searchSize, GridProperties.CellLenghtX - 1);
            int startY = Mathf.Max(0, point.CellMin.y - searchSize);
            int endY = Mathf.Min(point.CellMax.y + searchSize, GridProperties.CellLenghtY - 1);
            
            float sqrtRad = Mathf.Pow(point.Radius + UserProperties.PointMargin, 2);
            for (int y = startY; y <= endY; y++)
            {
                float cellY = Mathf.Abs(GridProperties.CellSize * y) + GridProperties.PositionOffset.y;
                float powY = Mathf.Pow(cellY - point.WorldPosition.y, 2);

                for (int x = startX; x <= endX; x++)
                {
                    if (GetCellValue(x, y) == 0)
                    {
                        float cellX = Mathf.Abs(GridProperties.CellSize * x) + GridProperties.PositionOffset.x;
                        float powX = Mathf.Pow(cellX - point.WorldPosition.x, 2);
                        float dstToCenter = powX + powY;
                        if (dstToCenter < sqrtRad)
                        {
                            SetCellValue(x, y, PointsInternal.Count);
                        } 
                    }
                }
            }
        }
        
        private Candidate CreateCandidateFrom(PointGrid point, int currentTry)
        {
            float candidateRadius = UserProperties.RadiusProvider.GetRandomRadius(currentTry, UserProperties.Tries);
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: point.Radius + UserProperties.PointMargin,
                    candidateRadius: candidateRadius + UserProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = candidateRadius,
                Margin = UserProperties.PointMargin
            };
        }
        
        private bool IsCandidateValid(Candidate candidate, Vector2Int candidateCellMax, Vector2Int candidateCellMin)
        {
            int searchSize = Mathf.RoundToInt(candidate.Radius / GridProperties.CellSize);
            int startX = Mathf.Max(0, candidateCellMin.x - searchSize);
            int endX = Mathf.Min(candidateCellMax.x + searchSize, GridProperties.CellLenghtX - 1);
            int startY = Mathf.Max(0, candidateCellMin.y - searchSize);
            int endY = Mathf.Min(candidateCellMax.y + searchSize, GridProperties.CellLenghtY - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                int coordStart = FlatCoordinates(startX, y);
                int coordEnd = FlatCoordinates(endX, y);
                for (int x = coordStart; x <= coordEnd; x++)
                {
                    int pointIndex = GetCellValue(x);
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
    }
}