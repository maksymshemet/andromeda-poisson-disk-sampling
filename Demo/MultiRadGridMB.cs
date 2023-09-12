using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class MultiRadGridMB : MonoBehaviour
    {
        [Header("MinMax Radii")]
        public float MinRadius;
        public float MaxRadius;

        [Header("Predefined Radii")] 
        public bool UsePredefinedRadii;
        public float[] PredefinedRadii;
        
        [Header("Other")]
        public PointMultiRadSphere Pref;

        public PointsLocation PointsLocation;
        public AnimationCurve RadiusPerTryCurve;
        public float PointMargin;
        public int Tries = 20;
        public Vector2Int Size;
        public Vector3 PositionOffset;
        public bool UseRandomSeed = true;
        public int RandomSeed;
        
        public bool Trigger;
        private bool _trigger;

        private GridMultiRad _grid;
        private List<PointMultiRadSphere> _spheres;
        
        private void Awake()
        {
            _trigger = Trigger;
            _spheres = new List<PointMultiRadSphere>();
        }

        private void Do()
        {
            foreach (PointMultiRadSphere sphere in _spheres)
            {
                Destroy(sphere.gameObject);
            }
            
            _spheres.Clear();

            GridBuilderMultiRadius builder = new GridBuilderMultiRadius()
                .WithPointProperties(pointProps =>
                {
                    pointProps
                        .WithRadiusPerTryCurve(RadiusPerTryCurve)
                        .WithTries(Tries)
                        .WithMargin(PointMargin);

                    if (UsePredefinedRadii)
                    {
                        pointProps.WithRadii(PredefinedRadii);
                    }
                    else
                    {
                        pointProps.WithRadii(MinRadius, MaxRadius);
                    }
                })
                .WithGridProperties(gridPros => gridPros
                    .WithSize(Size)
                    .WithPointsLocation(PointsLocation)
                    .WithPositionOffset(PositionOffset)
                );

            _grid = builder.Build();
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
            Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalMilliseconds} ms ({_grid.Points.Count()} points)");
            // foreach (Point point in _grid2.Points)
            // {
            //     PointSphere mb = Instantiate(Pref);
            //     mb.Init(point, _grid2);
            //     _spheres.Add(mb);
            // }

            Camera.main.transform.position = _grid.GridProperties.Center;
        }

        private void GridOnOnPointCreated(GridMultiRad grid, PointGrid point)
        {
            PointMultiRadSphere mb = Instantiate(Pref, transform, true);
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
            
            // Gizmos.DrawWireCube(
            //     center: _grid2.GridProperties.Center,
            //     size: new Vector3(_grid2.UserProperties.Size.x, _grid2.UserProperties.Size.y)
            // );
            
            float offsetX = _grid.GridProperties.CellSize;
            float offsetY = _grid.GridProperties.CellSize;
            
            float width = _grid.GridProperties.CellSize + _grid.GridProperties.CellLenghtX;
            float height = _grid.GridProperties.CellSize + _grid.GridProperties.CellLenghtY;

            for (var x = 0; x < _grid.GridProperties.CellLenghtX; x++)
            {
                var from = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y);
                var to = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y + height);
            
                Gizmos.DrawLine(@from, to);
            }

            for (var y = 0; y < _grid.GridProperties.CellLenghtY; y++)
            {
                var from = new Vector2(PositionOffset.x, PositionOffset.y + offsetY * y);
                var to = new Vector2(PositionOffset.x + width, PositionOffset.y + offsetY * y);
            
                Gizmos.DrawLine(@from, to);
            }
            
        }
    }
}