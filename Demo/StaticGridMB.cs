using System;
using System.Collections.Generic;
using System.Diagnostics;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
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

        private GridStatic _grid;
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
            
            _grid = new GridBuilderConstRadius()
                .WithPointProperties(x => x
                    .WithRadius(Radius)
                    .WithTries(Tries)
                    .WithMargin(PointMargin))
                .WithGridProperties(gridPros => gridPros
                    .WithSize(Size)
                    .WithPointsLocation(PointsLocation)
                    .WithPositionOffset(PositionOffset)
                )
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
            
            _grid.Fill();
            sw.Stop();
            Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalMilliseconds} ms ({_grid.Points.Count} points)");
            
            Camera.main.transform.position = _grid.GridProperties.Center;
        }

        private void GridOnOnPointCreated(GridStatic grid, PointGrid point)
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
    }
}