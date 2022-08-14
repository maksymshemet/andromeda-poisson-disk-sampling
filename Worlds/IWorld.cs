using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Worlds
{
    public interface IWorld
    {
        public GridProperties GridProperties { get; }
        public Vector2 ChunkSize { get; }
        public IEnumerable<IWorldGrid> Grids { get; }
        
        List<Point> AddGrid(Vector2Int chunkPosition, bool fill = true);
        IWorldGrid GetGrid(Vector2Int vector2Int);
        Point GetPoint(int chunkX, int chunkY, int cellX, int cellY);
        Point GetPoint(Vector2Int chunkCoord, int cellX, int cellY);
        void SetCellValue(WorldCoordinate coordinate, int value);
        int AddPoint(WorldCoordinate coordinate, Point point);
        WorldCoordinate GetRealWorldCoordinate(int chunkX, int chunkY, int cellX, int cellY);
        WorldCoordinate GetRealWorldCoordinate(Vector2Int chunkPosition, int cellX, int cellY);
    }
}