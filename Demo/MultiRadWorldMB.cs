using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class MultiRadWorldMB : MonoBehaviour
    {
        public WorldPointSphere Pref;
        
        public int ChunkCellsCount;
        public int Tries = 20;
        public float MinRadius;
        public float MaxRadius;
        public AnimationCurve RadiusPerTryCurve;
        public Vector2Int[] Chunks;
        public Vector2Int ChunkWorldCoordinates;
        public Vector2Int[] WorldCoordinates;

        public bool Trigger;
        private bool _trigger;
        
        private WorldMultiRad _world;

        private List<WorldCoordinates> _worldCoordinates;


        [Header("WorldPosition To Cell")] 
        public Vector3 WorldPosition;
        public bool TriggerWorldPosition;
        
        [Header("Random")] 
        public bool UseRandomSeed = true;
        public int RandomSeed;
        
        private bool _triggerWorldPosition;
        
        private List<WorldPointSphere> _spheres;
        
        private void Awake()
        {
            _trigger = Trigger;
            _triggerWorldPosition = TriggerWorldPosition;
            
            _spheres = new List<WorldPointSphere>();
        }

        private void Update()
        {
            if (_triggerWorldPosition != TriggerWorldPosition)
            {
                _triggerWorldPosition = TriggerWorldPosition;
                if (_world != null)
                    Debug.Log(_world.WorldPositionToWorldCoordinates(WorldPosition, method: WorldToCellPositionMethod.Floor));
            }
            
            if(_trigger == Trigger) return;
            
            foreach (WorldPointSphere sphere in _spheres)
            {
                Destroy(sphere.gameObject);
            }
            
            _spheres.Clear();
            
            _trigger = Trigger;
            _worldCoordinates = new List<WorldCoordinates>();

            _world = new WorldBuilderMultiRadius()
                .WithPointProperties(pp => pp
                    .WithRadii(MinRadius, MaxRadius)
                    .WithTries(Tries)
                    .WithRadiusPerTryCurve(RadiusPerTryCurve)
                )
                .WithChunkSize(ChunkCellsCount)
                .Build();
            // _world = new World2BuilderMultiRad()
            //     .WithRadius(MinRadius, MaxRadius)
            //     .WithTries(Tries)
            //     .WithRadiusPerTryCurve(RadiusPerTryCurve)
            //     .WithChunkSize(ChunkCellsCount)
            //     .Build();
            
            _world.OnPointCreated += Grid2OnOnPointCreated;
            
            int ts = DateTime.Now.Millisecond;
            Debug.LogWarning($"seed: {ts}");

            Random.InitState(UseRandomSeed ? ts : RandomSeed);
            var sw = new Stopwatch();
            sw.Start();
            
            foreach (Vector2Int chunk in Chunks)
            {
                _world.CreateGrid(chunk).Fill();
                // break;
            }
            
            sw.Stop();
            int pointCount = _world.Grids.Values.SelectMany(x => x.Points).Distinct().Count();
            Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalSeconds} s ({pointCount} points)");
            
            foreach (Vector2Int coordinate in WorldCoordinates)
            {
                WorldCoordinates a = _world.RelativeToWorldCoordinates(coordinate, ChunkWorldCoordinates);
                _worldCoordinates.Add(a);
                Debug.Log(a);
            }

          
            // _world.Grids[Chunks[0]].Fill();
        }

        private void Grid2OnOnPointCreated(WorldGridMultiRad arg1, PointGrid arg2)
        {
            WorldPointSphere mb = Instantiate(Pref, transform, true);
            mb.Init(_world, arg2);
            _spheres.Add(mb);
        }

        private void OnDrawGizmos()
        {
            if(_world == null) return;

            foreach (WorldGridMultiRad grid in _world.Grids.Values)
            {
                Gizmos.color = Color.magenta;
            
                Gizmos.DrawWireCube(
                    center: grid.ChunkProperties.WorldCenter,
                    size: new Vector3(_world.GridProperties.Size.x, _world.GridProperties.Size.y)
                );
                
                
                            
                Gizmos.color = Color.yellow;
            
                var offsetX = _world.GridProperties.CellSize;
                var offsetY = _world.GridProperties.CellSize;
            
                for (int x = 0; x < _world.GridProperties.CellLenghtX; x++)
                {
                    var from = new Vector2(
                        x: grid.ChunkProperties.WorldPosition.x + offsetX * x, 
                        y: grid.ChunkProperties.WorldPosition.y);
                    var to = new Vector2(
                        x: grid.ChunkProperties.WorldPosition.x + offsetX * x, 
                        y: grid.ChunkProperties.WorldPosition.y + _world.GridProperties.CellLenghtY * _world.GridProperties.CellSize);
            
                    Gizmos.DrawLine(@from, to);
                }
            
                for (int y = 0; y < _world.GridProperties.CellLenghtY; y++)
                {
                    var from = new Vector2(
                        x: grid.ChunkProperties.WorldPosition.x, 
                        y: grid.ChunkProperties.WorldPosition.y + offsetY * y);
                    var to = new Vector2(
                        x: grid.ChunkProperties.WorldPosition.x + _world.GridProperties.CellLenghtX * _world.GridProperties.CellSize, 
                        y: grid.ChunkProperties.WorldPosition.y + offsetY * y);
            
                    Gizmos.DrawLine(@from, to);
                }
                
                // Gizmos.color = Color.white;
                // foreach (Candidate gridCandidate in grid.Candidates)
                // {
                //     Gizmos.DrawWireSphere(gridCandidate.WorldPosition, gridCandidate.Radius);
                // }
            }
        }
    }
}