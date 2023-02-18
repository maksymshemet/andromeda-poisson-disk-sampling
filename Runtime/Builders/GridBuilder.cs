using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public abstract class GridBuilder<TPointProperties, TPointPropertiesBuilder, TGrid, TSelf> : IBuilder<TGrid>
        where TPointProperties : PointProperties
        where TPointPropertiesBuilder : PointPropertiesBuilder<TPointProperties, TPointPropertiesBuilder>, new()
        where TSelf : GridBuilder<TPointProperties, TPointPropertiesBuilder, TGrid, TSelf>
    {
        protected TPointPropertiesBuilder PointPropertiesBuilder;
        protected GridPropertiesBuilderForGrid GridPropertiesBuilder;
        
        public TSelf WithPointProperties(Action<TPointPropertiesBuilder> builder)
        {
            var b = new TPointPropertiesBuilder();
            builder(b);
            return WithPointProperties(b);
        }
        
        public TSelf WithPointProperties(TPointPropertiesBuilder builder)
        {
            PointPropertiesBuilder = builder;
            return (TSelf) this;
        }
        
        public TSelf WithGridProperties(Action<GridPropertiesBuilderForGrid> builder)
        {
            var b = new GridPropertiesBuilderForGrid();
            builder(b);
            return WithGridProperties(b);
        }
        
        public TSelf WithGridProperties(GridPropertiesBuilderForGrid builder)
        {
            GridPropertiesBuilder = builder;
            return (TSelf) this;
        }
        
        public abstract TGrid Build();
    }

    public class GridBuilderConstRadius : 
        GridBuilder<PointPropertiesConstRadius, PointPropertiesBuilderConsRadius, GridStatic, GridBuilderConstRadius>
    {
        public override GridStatic Build()
        {
            PointPropertiesConstRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = GridPropertiesBuilder.Build();
            
            float cellSize = (pointProperties.Radius + pointProperties.PointMargin) / 2f;

            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtY = Mathf.CeilToInt(gridProperties.Size.y / cellSize);
            gridProperties.CellLenghtX = Mathf.CeilToInt(gridProperties.Size.x / cellSize);
            gridProperties.Center = new Vector3(gridProperties.Size.x / 2f, gridProperties.Size.y / 2f) 
                                + gridProperties.PositionOffset;

            return new GridStatic(pointProperties, gridProperties);
        }
    }
    
    public class GridBuilderMultiRadius : 
        GridBuilder<PointPropertiesMultiRadius, PointPropertiesBuilderMultiRadius, GridMultiRad, GridBuilderMultiRadius>
    {
        public override GridMultiRad Build()
        {
            PointPropertiesMultiRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = GridPropertiesBuilder.Build();
            
            float cellSize = (pointProperties.RadiusProvider.MinRadius + pointProperties.PointMargin) / 2f;

            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtY = Mathf.CeilToInt(gridProperties.Size.y / cellSize);
            gridProperties.CellLenghtX = Mathf.CeilToInt(gridProperties.Size.x / cellSize);
            gridProperties.Center = new Vector3(gridProperties.Size.x / 2f, gridProperties.Size.y / 2f) 
                                    + gridProperties.PositionOffset;

            return new GridMultiRad(pointProperties, gridProperties);
        }
    }
}