using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class PointWorld : PointGrid
    {
        public Vector2Int ChunkPosition;
        
        public WorldCoordinates CellMinWorld;
        public WorldCoordinates CellMaxWorld;
        
        public override bool Equals(object obj)
        {
            if (obj is PointWorld point)
            {
                return ChunkPosition.Equals(point.ChunkPosition)
                       && WorldPosition.Equals(point.WorldPosition)
                       && Radius.Equals(point.Radius);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (WorldPosition, Radius, ChunkPosition).GetHashCode();
        }

        public override string ToString()
        {
            return $"Point{ChunkPosition}: wp{WorldPosition} r{Radius}]";
        }
    }
}