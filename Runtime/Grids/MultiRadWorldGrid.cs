using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public class MultiRadWorldGrid : AbstractWorldGrid
    {
        private Dictionary<int, Dictionary<Vector2Int,int>> _cached;
        
        public IFillPoints<IWorldGrid, PointWorld> PointFiller { get; set; }

        public MultiRadWorldGrid(IWorld world, Vector2Int chunkPosition, GridProperties gridProperties, 
            ICandidateValidator<IWorldGrid, PointWorld> candidateValidator, IPointSettings pointSettings) 
            : base(world, chunkPosition, gridProperties, candidateValidator, pointSettings)
        {
        }

        protected override void PostPointCreated(in PointWorld point, int pointIndex)
        {
            var cached = PointFiller?.FillPoints( this, point);
            if (_cached == null)
                _cached = new Dictionary<int, Dictionary<Vector2Int, int>>();
            
            _cached[pointIndex] = cached;
            
            base.PostPointCreated(point, pointIndex);
        }

        protected override void OnPointRemoved(PointWorld point, int cellValue)
        {
            if (_cached.TryGetValue(cellValue, out var cached))
            {
                foreach (var pair in cached)
                {
                    var chunks = pair.Key;
                    var cell = pair.Value;

                    var grid = World.GetGrid(chunks);
                    grid.TestRemoveCellValue(cell);
                }
            }
        }
    }
}