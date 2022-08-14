using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Worlds
{
    public interface IWorld
    {
        public GridProperties GridProperties { get; }
        public Vector2 ChunkSize { get; }
        public IEnumerable<IWorldGrid> Grids { get; }
        IPointSettings PointSettings { get; set; }
        PointWorld TrySpawnPointFrom(PointWorld spawnPoint);
        List<PointWorld> CreateGrid(Vector2Int chunkPosition, bool fill = true);
        IWorldGrid GetGrid(Vector2Int vector2Int);
        PointWorld GetPoint(int chunkX, int chunkY, int cellX, int cellY);
        PointWorld GetPoint(Vector2Int chunkCoord, int cellX, int cellY);
        int AddPoint(WorldCoordinate coordinate, PointWorld point);
        WorldCoordinate GetRealWorldCoordinate(int chunkX, int chunkY, int cellX, int cellY);
        WorldCoordinate GetRealWorldCoordinate(Vector2Int chunkPosition, int cellX, int cellY);
    }
}