using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public class WorldMultiRad : WorldAbstract<PointPropertiesMultiRadius, WorldGridMultiRad, WorldMultiRad>
    {
        public WorldMultiRad(GridProperties gridProperties, PointPropertiesMultiRadius userProperties) 
            : base(gridProperties, userProperties)
        {
        }

        protected override WorldGridMultiRad CreateGrid(Vector2Int chunkPosition, WorldChunkProperties chunkProperties)
        {
            return new WorldGridMultiRad(this, chunkPosition, chunkProperties);
        }

    }
}