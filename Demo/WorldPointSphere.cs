using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds;
using UnityEngine;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class WorldPointSphere : MonoBehaviour
    {
        public Color GridCellsColor = Color.red;
        public float GridCellsRadius = 0.1f;
        
        private World _world;
        private WorldMultiRad _world2;
        private PointGrid _point;
    
        public void Init(World world, PointGrid point)
        {
            name = $"[{point.WorldPosition}]";
            transform.position = point.WorldPosition;
            transform.localScale = new Vector3(point.Radius, point.Radius, point.Radius) * 2;

            _world = world;
            _point = point;
        }
        
        public void Init(WorldMultiRad world, PointGrid point)
        {
            name = $"[{point.WorldPosition}]";
            transform.position = point.WorldPosition;
            transform.localScale = new Vector3(point.Radius, point.Radius, point.Radius) * 2;

            _world2 = world;
            _point = point;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GridCellsColor;
            
            if (_world != null)
            {
                foreach (WorldGrid grid in _world.Grids.Values)
                {
                    for (int y = 0; y < grid.GridProperties.CellLenghtX; y++)
                    {
                        for (int x = 0; x < grid.GridProperties.CellLenghtX; x++)
                        {
                            var cellValue = grid.GetCellValue(x, y);
                            if (cellValue != 0)
                            {
                                PointGrid p = grid.Points[cellValue - 1];
                                if (p == _point)
                                {
                                    Gizmos.DrawSphere(new Vector3(
                                        (grid.GridProperties.CellSize * x) + grid.ChunkProperties.WorldPosition.x,
                                        (grid.GridProperties.CellSize * y) + grid.ChunkProperties.WorldPosition.y
                                    ), GridCellsRadius);
                                }
                            }
                        }
                    }
                }
            }
            else if (_world2 != null)
            {
                foreach (WorldGridMultiRad grid in _world2.Grids.Values)
                {
                    for (int y = 0; y < grid.GridProperties.CellLenghtX; y++)
                    {
                        for (int x = 0; x < grid.GridProperties.CellLenghtX; x++)
                        {
                            var cellValue = grid.GetCellValue(x, y);
                            if (cellValue != 0)
                            {
                                PointGrid p = grid.Points[cellValue - 1];
                                if (p == _point)
                                {
                                    Gizmos.DrawSphere(new Vector3(
                                        (grid.GridProperties.CellSize * x) + grid.ChunkProperties.WorldPosition.x,
                                        (grid.GridProperties.CellSize * y) + grid.ChunkProperties.WorldPosition.y
                                    ), GridCellsRadius);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}