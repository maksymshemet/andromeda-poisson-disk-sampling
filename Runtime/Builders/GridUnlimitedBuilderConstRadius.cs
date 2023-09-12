using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class GridUnlimitedBuilderConstRadius<TPoint> : 
        GridBuilder<PointPropertiesConstRadius, PointPropertiesBuilderConsRadius, 
            GridStaticUnlimited<TPoint>, TPoint, GridUnlimitedBuilderConstRadius<TPoint>>
        where TPoint : PointGrid, new()
    {
        public override GridStaticUnlimited<TPoint> Build()
        {
            PointPropertiesConstRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = BuildGridProperties();
            
            float cellSize = (pointProperties.Radius + pointProperties.PointMargin) / 2f;

            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtY = Mathf.CeilToInt(gridProperties.Size.y / cellSize);
            gridProperties.CellLenghtX = Mathf.CeilToInt(gridProperties.Size.x / cellSize);
            gridProperties.Center = new Vector3(gridProperties.Size.x / 2f, gridProperties.Size.y / 2f) 
                                    + gridProperties.PositionOffset;

            return new GridStaticUnlimited<TPoint>(pointProperties, gridProperties)
            {
                CustomBuilder = CustomBuilder
            };
        }
    }
    
    public class GridUnlimitedBuilderConstRadius : GridUnlimitedBuilderConstRadius<PointGrid>
    {
        
    }
}