using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class WorldCoordinates
    {
        public Vector2Int ChunkPosition;
        public Vector2Int CellPosition;

        public override string ToString()
        {
            return $"[{ChunkPosition}] {CellPosition}";
        }
    }
}