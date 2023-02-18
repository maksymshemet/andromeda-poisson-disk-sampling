using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public class World : WorldAbstract<PointPropertiesConstRadius, WorldGrid, World>
    {
        public World(GridProperties gridProperties, PointPropertiesConstRadius userProperties) 
            : base(gridProperties, userProperties)
        {

        }

        protected override WorldGrid CreateGrid(Vector2Int chunkPosition, WorldChunkProperties chunkProperties)
        {
            return new WorldGrid(this, chunkPosition, chunkProperties);
        }
    }
}