using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public abstract class WorldGridAbstract<TWorld, TGridUserProperty, TSelf> 
        : GridAbstract<PointWorld, TGridUserProperty, TSelf>
        where TGridUserProperty : PointProperties
        where TWorld : WorldAbstract<TGridUserProperty, TSelf, TWorld>
        where TSelf : WorldGridAbstract <TWorld, TGridUserProperty, TSelf>
    {
        
        protected readonly TWorld World;
        
        public Vector2Int ChunkCoordinate { get; }
        public WorldChunkProperties ChunkProperties { get; }
        
        public WorldGridAbstract(TWorld world, Vector2Int chunkCoordinate, WorldChunkProperties chunkProperties,
            TGridUserProperty userProperties) 
            : base(userProperties, world.GridProperties)
        {
            World = world;
            ChunkCoordinate = chunkCoordinate;
            ChunkProperties = chunkProperties;
        }

        public abstract void Fill();
        
        public int TrySpawnPointFrom(PointWorld point, out PointWorld newPoint)
        {
            for (var i = 0; i < World.UserProperties.Tries; i++)
            {
                Candidate candidate = CreateCandidateFrom(point, i);

                WorldCoordinates candidateCellMax = World.WorldPositionToWorldCoordinates(candidate.WorldPosition,
                    WorldToCellPositionMethod.Ceil);
                WorldCoordinates candidateCellMin = World.WorldPositionToWorldCoordinates(candidate.WorldPosition,
                    WorldToCellPositionMethod.Floor);

                if (candidateCellMax.ChunkPosition != ChunkCoordinate &&
                    candidateCellMin.ChunkPosition != ChunkCoordinate)
                {
                    continue;
                }

                if (IsCandidateValid(candidate, candidateCellMax, candidateCellMin))
                {
                    newPoint = new PointWorld
                    {
                        WorldPosition = candidate.WorldPosition,
                        Radius = candidate.Radius,
                        Margin = candidate.Margin,
                        ChunkPosition = ChunkCoordinate
                    };
                    AddPoint(newPoint);
                    return PointsInternal.Count - 1;
                }
            }

            newPoint = default;
            
            return -1;
        }

        internal void AddPoint(in PointWorld point, Vector2Int cellPosition)
        {
            if (PointsInternal.Count == 0 || PointsInternal[^1] != point)
            {
                PointsInternal.Add(point);
            }
            
            Cells[FlatCoordinates(cellPosition)] = PointsInternal.Count;
        }
        
        private bool IsCandidateValid(Candidate candidate, WorldCoordinates candidateCellMax, WorldCoordinates candidateCellMin)
        {
            int radius = GetSearchSize(candidate.Radius);
            HashSet<PointWorld> points = World.GetPointsAround(candidateCellMin, candidateCellMax, radius);
            foreach (PointWorld point in points)
            {
                if (candidate.IsIntersectWithPoint(point))
                {
                    return false;
                }
            }

            return true;
        }

        protected abstract Candidate CreateCandidateFrom(PointWorld point, int currentTry);
        
        private int GetSearchSize(float pointRadius) => 
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / World.GridProperties.CellSize));
        
    }
}