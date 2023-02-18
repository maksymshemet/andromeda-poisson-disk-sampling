using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridStatic : Grid<PointPropertiesConstRadius, GridStatic>
    {
        public GridStatic(PointPropertiesConstRadius userProperties, GridProperties gridProperties) 
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
                Radius = UserProperties.Radius,
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
                Candidate candidate = CreateCandidateFrom(point);

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

        private Candidate CreateCandidateFrom(PointGrid point)
        {
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: UserProperties.Radius + UserProperties.PointMargin,
                    candidateRadius: UserProperties.Radius + UserProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = UserProperties.Radius,
                Margin = UserProperties.PointMargin
            };
        }

        private bool IsCandidateValid(Candidate candidate, Vector2Int candidateCellMax, Vector2Int candidateCellMin)
        {
            int searchSize = GetSearchSize(candidate.Radius);
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
        
        private int GetSearchSize(float pointRadius) => 
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridProperties.CellSize));
    }
}