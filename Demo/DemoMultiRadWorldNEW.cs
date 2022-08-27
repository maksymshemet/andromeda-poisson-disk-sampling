using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling.Demo
{
    public class DemoMultiRadWorldNEW : MonoBehaviour
    {
        public float PointMargin = 0;
        public float MinRadius = 1;
        public float MaxRadius = 2;
        public AnimationCurve RadiusCurve;
        
        public Vector2Int RegionSize = new Vector2Int(10, 10);
        public int Tries = 20;
        
        [Header("Colors")]
        public Color GridColor = new Color(.5f, .5f, .5f, .5f);
        public Color PointColor = new Color(.5f, .5f, .5f, .5f);

        
        [Header("Trigger")] public bool Trigger;

        private List<PointWorld> _points;
        private WorldMultiRad _world;

        private static bool _cachedTrigger;


        private void OnValidate()
        {
            if (_cachedTrigger != Trigger)
            {
                _world = Factory.CreateWorldNEW(RegionSize, MinRadius, MaxRadius, Tries, PointMargin, RadiusCurve);
                
                // 595, 649, 376
                var ts = DateTime.Now.Millisecond;
                Debug.LogWarning($"seed: {ts}");
            
                // 909
                Random.InitState(ts);
                // Random.InitState(201);
                // Random.InitState(500);
                
                var sw = new Stopwatch();
                sw.Start();
                _points = _world.CreateGrid(new Vector2Int(0, 0)).Fill();
                _points.AddRange(_world.CreateGrid(new Vector2Int(-1, 0)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(-1, 1)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(-1, -1)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(0, -1)).Fill());
                // _world.AddGrid(new Vector2Int(0, -1));
                _points.AddRange(_world.CreateGrid(new Vector2Int(0, 1)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(1, -1)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(1, 0)).Fill());
                _points.AddRange(_world.CreateGrid(new Vector2Int(1, 1)).Fill());

                _points = _points.Distinct().ToList();
                
                sw.Stop();

                var d = new Dictionary<Vector3, List<Point>>();

                foreach (var point in _points)
                {
                    if (d.TryGetValue(point.WorldPosition, out var list))
                    {
                        list.Add(point);
                    }
                    else
                    {
                        d[point.WorldPosition] = new List<Point> { point };
                    }
                }
                
                
                Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalMilliseconds} ms ({_points.Count} points)");
                
                _cachedTrigger = Trigger;
            }
        }

        private void OnDrawGizmos()
        {
            if (_world == null) return;
            
            ShowGrid();

            if (_points == null) return;
            
            foreach (var point in _points)
            {
                if (point.ChunkPosition.x < 0)
                {
                    Gizmos.color = new Color(0f, 0f, 1f, PointColor.a);
                }
                else if (point.ChunkPosition.y < 0)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, PointColor.a);

                }
                else
                {
                    Gizmos.color = PointColor;
                }
               
                Gizmos.DrawSphere(point.WorldPosition, point.Radius);
                Gizmos.color = new Color(1f, 0f, 0f, PointColor.a);
                Gizmos.DrawSphere(point.WorldPosition, 0.03f);
            }


            // DrawGrid(_world.GetGrid(new Vector2Int(-1, 0)));
            // DrawGrid(_world.GetGrid(new Vector2Int(0, -1)));
            // DrawGrid(_world.GetGrid(new Vector2Int(0, 0)));
            // DrawGrid(_world.GetGrid(new Vector2Int(0, 1)));s
            // DrawGrid(_world.GetGrid(new Vector2Int(-1, 0)));
            // DrawGrid(_world.GetGrid(new Vector2Int(-1, 1)));
            // DrawGrid(_world.GetGrid(new Vector2Int(1, 1)));
            // DrawGrid(_world.GetGrid(new Vector2Int(1, 0)));
        }

        private void DrawGrid(IGrid grid)
        {
            if(grid == null) return;
            
            for (int y = 0; y < grid.GridProperties.CellHeight; y++)
            {
                for (int x = 0; x < grid.GridProperties.CellWidth; x++)
                {
                    var c  = grid.GetCellValue(x, y);
                    
                    if(c == 0) continue;
                    
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(
                        new Vector2(
                            grid.WorldPositionOffset.x + (x * grid.GridProperties.CellSize), 
                            grid.WorldPositionOffset.y+ (y * grid.GridProperties.CellSize)),0.04f);
                }
            }
        }
        
        private void ShowGrid()
        {
            Gizmos.color = Color.grey;

            float worldOffsetX = 0;
            float worldOffsetY = 0;
            
            Gizmos.color = GridColor;

            var offsetX = _world.GridProperties.CellSize;
            var offsetY = _world.GridProperties.CellSize;

            foreach (var grid in _world.Grids)
            {

                worldOffsetX = _world.ChunkSize.x * grid.ChunkPosition.x;
                worldOffsetY = _world.ChunkSize.y * grid.ChunkPosition.y;
                
                Gizmos.color = new Color(1f, 0f, 0f, GridColor.a);
                Gizmos.DrawWireCube(
                    new Vector3((_world.ChunkSize.x / 2f) + worldOffsetX, 
                        (_world.ChunkSize.y / 2f) + worldOffsetY), 
                    new Vector3(_world.ChunkSize.x, _world.ChunkSize.y));
                
                Gizmos.color = GridColor;
                for (int x = 0; x < grid.GridCore.CellWidth; x++)
                {
                    var from = new Vector2(worldOffsetX + offsetX * x,worldOffsetY);
                    var to = new Vector2(worldOffsetX + offsetX * x, worldOffsetY + RegionSize.y);
                    Gizmos.DrawLine(@from, to);
                        
                }
                
                for (int y = 0; y < grid.GridCore.CellHeight; y++)
                {
                    var from = new Vector2(worldOffsetX, worldOffsetY + offsetY * y);
                    var to = new Vector2(worldOffsetX + RegionSize.x, worldOffsetY + offsetY * y);
                
                    Gizmos.DrawLine(@from, to);
                }
            }
        }
    }
}