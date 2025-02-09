using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Points;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Radii;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors.MultiRadiiSelectors;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace andromeda_poisson_disk_sampling.Demo2
{
    [Serializable]
    public class PredefinedRadii
    {
        public float Radius;
        public float Margin = 0;
        
    }
    public class MultiRadGridMB : MonoBehaviour
    {
        [Header("MinMax Radii")]
        public float MinRadius;
        public float MaxRadius;

        [Header("Predefined Radii")] 
        public bool UsePredefinedRadii;
        public PredefinedRadii[] PredefinedRadii;
        public PredefinedRadiusSelectType RadiusSelectType;
            
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
        public bool CreateGameObjects = true;
        
        public bool Trigger;
        private bool _trigger;

        private IDPSGrid _grid;
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
            
            IRadiusSelectorMulti radiusSelector = null;
            if (UsePredefinedRadii)
            {
                radiusSelector = new RadiusSelectorMultiPredefined(new RadiusPropertiesPredefined
                {
                    PredefinedRadii = PredefinedRadii
                        .Select(x => new PointSize
                        {
                            Radius = x.Radius,
                            Margin = x.Margin
                        }).ToArray(),
                    RadiusSelectType = RadiusSelectType,
                    RadiusPerTryCurve = RadiusPerTryCurve
                });
            }
            else
            {
                radiusSelector = new RadiusSelectorMultiRandom(new RadiusPropertiesMinMax
                {
                    MinRadius = MinRadius,
                    MaxRadius = MaxRadius,
                    RadiusPerTryCurve = RadiusPerTryCurve
                });
            }

            GridBuilderMultiRadius builder = GridBuilder.MultiRadius()
                .WithPointProperties(new PointPropertiesMultiRadius
                {
                    RadiusSelector = radiusSelector
                })
                .WithGridProperties(grid =>
                {
                    grid.Size = Size;
                    grid.PointsLocation = PointsLocation;
                    grid.PositionOffset = PositionOffset;
                    grid.Tries = Tries;
                    grid.PointMargin = PointMargin;
                });

            _grid = builder.Build();
            transform.position = PositionOffset;
            // _grid.OnPointCreated += GridOnOnPointCreated;
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

            if (CreateGameObjects)
            {
                foreach (DPSPoint point in _grid.Points)
                {
                    GridOnOnPointCreated(_grid, point);
                }
            }

            Camera.main.transform.position = _grid.GridProperties.Center;
        }
        
        public void Fill()
        {
            Vector3 fakeWorldPosition = new Vector3(
                x: _grid.GridProperties.Size.x / 2f, 
                y: _grid.GridProperties.Size.y / 2f) + _grid.GridProperties.PositionOffset;

            var candidate = new Candidate{
                WorldPosition=fakeWorldPosition, 
                Size = new PointSize {
                    Radius = UsePredefinedRadii ? PredefinedRadii[0].Radius : MinRadius, 
                    Margin = UsePredefinedRadii ? PredefinedRadii[0].Margin : PointMargin
                }
            };
            
            DPSPoint point = _grid.TryAddPoint(candidate);
            if (point == null)
            {
                throw new Exception("Couldn't spawn the point");
            }

            var spawnPoints = new List<int> { 0 };
            
            do
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                int spawnPointIndex = spawnPoints[spawnIndex];
                DPSPoint spawnPoint = _grid.GetPointByIndex(spawnPointIndex);
                
                point = _grid.TrySpawnPointFrom(spawnPoint);
                if (point != null)
                {
                    spawnPoints.Add(point.Index);
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
        
        private void GridOnOnPointCreated(IDPSGrid grid, DPSPoint point)
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
            
            Bounds bounds = new Bounds(_grid.GridProperties.Center - PositionOffset, _grid.GridProperties.Size);
            
            for (var x = 0; x < _grid.GridProperties.CellLenghtX; x++)
            {
                // var from = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y);
                // var to = new Vector2(PositionOffset.x + offsetX * x, PositionOffset.y + height);
                //
                // Gizmos.DrawLine(@from, to);
                
                var from = new Vector2(bounds.min.x + offsetX * x, bounds.min.y);
                var to = new Vector2(bounds.min.x + offsetX * x, bounds.max.y);
                
                Gizmos.DrawLine(@from, to);
            }

            for (var y = 0; y < _grid.GridProperties.CellLenghtY; y++)
            {
                // var from = new Vector2(PositionOffset.x, PositionOffset.y + offsetY * y);
                // var to = new Vector2(PositionOffset.x + width, PositionOffset.y + offsetY * y);
                //
                // Gizmos.DrawLine(@from, to);
                
                var from = new Vector2(bounds.min.x, bounds.min.y + offsetY * y);
                var to = new Vector2(bounds.max.x, bounds.min.y + offsetY * y);
                
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