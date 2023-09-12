using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{

    public abstract class GridPropertiesBuilder<TSelf> : IBuilder<GridProperties> 
        where TSelf : GridPropertiesBuilder<TSelf>
    {
        protected GridProperties Properties { get; }

        protected GridPropertiesBuilder()
        {
            Properties = new GridProperties();
        }

        public TSelf WithPointsLocation(PointsLocation pointsLocation)
        {
            Properties.PointsLocation = pointsLocation;
            return (TSelf) this;
        }
        
        public TSelf WithPositionOffset(Vector3 positionOffset)
        {
            Properties.PositionOffset = positionOffset;
            return (TSelf) this;
        }

        public GridProperties Build()
        {
            return Properties;
        }
        
    }
    public class GridPropertiesBuilderForGrid : GridPropertiesBuilder<GridPropertiesBuilderForGrid>
    {
        public GridPropertiesBuilderForGrid WithSize(Vector2 size)
        {
            Properties.Size = size;
            return this;
        }
    }
    
    public class GridPropertiesBuilderForWorld : GridPropertiesBuilder<GridPropertiesBuilderForGrid>
    {
    }
}