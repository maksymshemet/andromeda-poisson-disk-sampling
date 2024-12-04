using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Points;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class StaticGridMB : MonoBehaviour
    {
        public PointSphere Pref;
        public float Radius;
        public float PointMargin;
        public PointsLocation PointsLocation;
        public int Tries = 20;
        public Vector2Int Size;
        public Vector3 PositionOffset;
        public bool UseRandomSeed = true;
        public int RandomSeed;

        public bool Trigger;
        private bool _trigger;

        private IGrid _grid;
        private List<PointSphere> _spheres;
        
        private void Awake()
        {
            _trigger = Trigger;
            _spheres = new List<PointSphere>();
        }

        private void Do()
        {
            foreach (PointSphere sphere in _spheres)
            {
                Destroy(sphere.gameObject);
            }
            
            _spheres.Clear();
            
            _grid = GridBuilder.ConstRadius()
                .WithPointProperties(new PointPropertiesConstRadius
                {
                    Radius = Radius,
                    PointMargin = PointMargin,
                })
                .WithGridProperties(grid =>
                {
                    grid.Size = Size;
                    grid.PointsLocation = PointsLocation;
                    grid.PositionOffset = PositionOffset;
                    grid.Tries = Tries;
                })
                // .WithGridProperties(new GridProperties
                // {
                //     Size = Size,
                //     PointsLocation = PointsLocation,
                //     PositionOffset = PositionOffset,
                //     Tries = Tries
                // })
                .Build();
            
            transform.position = PositionOffset;
            _grid.OnPointCreated += GridOnOnPointCreated;
            int ts = DateTime.Now.Millisecond;
            Debug.LogWarning($"seed: {ts}");
            
            // 909
            Random.InitState(UseRandomSeed ? ts : RandomSeed);
            // Random.InitState(321);
            // Random.InitState(500);
                
            var sw = new Stopwatch();
            sw.Start();
            
            Fill();
            
            sw.Stop();
            Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalMilliseconds} ms ({_grid.Points.Count()} points)");
            
            Camera.main.transform.position = _grid.GridProperties.Center;
        }
        
        
        public void Fill()
        {
            Vector3 fakeWorldPosition = new Vector3(
                x: _grid.GridProperties.Size.x / 2f, 
                y: _grid.GridProperties.Size.y / 2f) + _grid.GridProperties.PositionOffset;

            var candidate = new Candidate{
                WorldPosition=fakeWorldPosition, 
                Radius = Radius, 
                Margin = PointMargin
            };
            
            Point point = _grid.TryAddPoint(candidate);
            if (point == null)
            {
                throw new Exception("Couldn't spawn the point");
            }

            var spawnPoints = new List<int> { 1 };
            
            do
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                int spawnPointIndex = spawnPoints[spawnIndex];
                Point spawnPoint = _grid.GetPointByIndex(spawnPointIndex);
                
                point = _grid.TrySpawnPointFrom(spawnPoint);
                if (point != null)
                {
                    spawnPoints.Add(point.Index + 1);
                }
                else
                {
                     spawnPoints[spawnIndex] = spawnPoints[^1];
                     spawnPoints.RemoveAt(spawnPoints.Count - 1);
                }
                
#if UNITY_EDITOR
                if (EditorCheckForEndlessSpawn(spawnPoints)) break;
#endif
            }
            while (spawnPoints.Count > 0);
        }

        private void GridOnOnPointCreated(IGrid grid, Point point)
        {
            PointSphere mb = Instantiate(Pref, transform, true);
            mb.Init(point, grid);
            _spheres.Add(mb);
        }

        private void Update()
        {
            if (Trigger != _trigger)
            {
                _trigger = Trigger;
                Do();
            }
        }

        private void OnDrawGizmos()
        {
            if(_grid == null) return;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(
                center: _grid.GridProperties.Center - PositionOffset,
                size: new Vector3(_grid.GridProperties.Size.x, _grid.GridProperties.Size.y)
            );
            
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawWireCube(
                center: _grid.GridProperties.Center,
                size: new Vector3(_grid.GridProperties.Size.x, _grid.GridProperties.Size.y)
            );
            
            float offsetX = _grid.GridProperties.CellSize;
            float offsetY = _grid.GridProperties.CellSize;
            
            float width = _grid.GridProperties.CellSize + _grid.GridProperties.CellLenghtX;
            float height = _grid.GridProperties.CellSize + _grid.GridProperties.CellLenghtY;
            
            for (int x = 0; x <= _grid.GridProperties.CellLenghtX; x++)
            {
                var from = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y);
                var to = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y + height);
            
                Gizmos.DrawLine(@from, to);
            }
            
            for (int y = 0; y <= _grid.GridProperties.CellLenghtY; y++)
            {
                var from = new Vector2(PositionOffset.x, PositionOffset.y + offsetY * y);
                var to = new Vector2(PositionOffset.x + width, PositionOffset.y + offsetY * y);
            
                Gizmos.DrawLine(@from, to);
            }
            
        }
        
         private bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
         {
             if (_grid.GridProperties.CellLenghtX *_grid. GridProperties.CellLenghtY >= _grid.PointsCount) return false;
             
             Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
             return true;
         }
    }
}