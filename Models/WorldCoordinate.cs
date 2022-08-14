using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public readonly struct WorldCoordinate
    {
        public readonly Vector2Int ChunkPosition;
        public readonly int CellX;
        public readonly int CellY;

        public WorldCoordinate(Vector2Int chunkPosition, int cellX, int cellY)
        {
            ChunkPosition = chunkPosition;
            CellX = cellX;
            CellY = cellY;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorldCoordinate coord)
            {
                return ChunkPosition.Equals(coord.ChunkPosition)
                       && CellX.Equals(coord.CellX)
                       && CellY.Equals(coord.CellY);
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            return (ChunkPosition, CellX, CellY).GetHashCode();
        }
    }
}