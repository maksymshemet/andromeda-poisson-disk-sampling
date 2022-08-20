using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public class MultiRadWorldGrid : AbstractWorldGrid
    {
        public IFillPoints<IWorldGrid, PointWorld> PointFiller { get; set; }

        public MultiRadWorldGrid(IWorld world, Vector2Int chunkPosition, GridProperties gridProperties, 
            ICandidateValidator<IWorldGrid, PointWorld> candidateValidator, IPointSettings pointSettings) : base(world, chunkPosition, gridProperties, candidateValidator, pointSettings)
        {
        }

        protected override void PostPointCreated(in PointWorld point)
        {
            PointFiller?.FillPoints( this, point);
            
            base.PostPointCreated(point);
        }
    }
}