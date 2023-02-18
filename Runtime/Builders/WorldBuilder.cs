using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    
    public abstract class WorldBuilder<TPointProperties, TPointPropertiesBuilder, TWorld, TSelf> : IBuilder<TWorld>
        where TPointProperties : PointProperties
        where TPointPropertiesBuilder : PointPropertiesBuilder<TPointProperties, TPointPropertiesBuilder>, new()
        where TSelf : WorldBuilder<TPointProperties, TPointPropertiesBuilder, TWorld, TSelf>
    {
        protected TPointPropertiesBuilder PointPropertiesBuilder;
        protected GridPropertiesBuilderForWorld GridPropertiesBuilder;
        protected int ChunkCellsCount = 30;
        
        public TSelf WithChunkSize(int cells)
        {
            ChunkCellsCount = cells;
            return (TSelf) this;
        }
        
        public TSelf WithPointProperties(Action<TPointPropertiesBuilder> builder)
        {
            var b = new TPointPropertiesBuilder();
            builder(b);
            return WithPointProperties(b);
        }
        
        public TSelf WithPointProperties(TPointPropertiesBuilder builder)
        {
            PointPropertiesBuilder = builder;
            return (TSelf) this;;
        }
        
        public TSelf WithGridProperties(Action<GridPropertiesBuilderForWorld> builder)
        {
            var b = new GridPropertiesBuilderForWorld();
            builder(b);
            return WithGridProperties(b);
        }
        
        public TSelf WithGridProperties(GridPropertiesBuilderForWorld builder)
        {
            GridPropertiesBuilder = builder;
            return (TSelf) this;;
        }

        public abstract TWorld Build();

        protected GridProperties ConfigureGridProperties(float minRadius)
        {
            GridProperties gridProperties = GridPropertiesBuilder == null ? 
                new GridProperties() : GridPropertiesBuilder.Build();
            
            float cellSize = (minRadius) / 2f;
            var size = new Vector2(
                x: cellSize * ChunkCellsCount,
                y: cellSize * ChunkCellsCount);

            gridProperties.Size = size;
            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtX = ChunkCellsCount;
            gridProperties.CellLenghtY = ChunkCellsCount;
            gridProperties.Center = new Vector3(size.x / 2f, size.y / 2f);

            return gridProperties;
        }
    }
        
    public class WorldBuilderConstRadius 
        : WorldBuilder<PointPropertiesConstRadius, PointPropertiesBuilderConsRadius, World, WorldBuilderConstRadius>
    {
        public override World Build()
        {
            PointPropertiesConstRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = ConfigureGridProperties(pointProperties.Radius);

            return new World(gridProperties, pointProperties);
        }
    }
    
    public class WorldBuilderMultiRadius
        : WorldBuilder<PointPropertiesMultiRadius, PointPropertiesBuilderMultiRadius,
            WorldMultiRad, WorldBuilderMultiRadius>
    {
        public override WorldMultiRad Build()
        {
            PointPropertiesMultiRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = ConfigureGridProperties(pointProperties.RadiusProvider.MinRadius);

            return new WorldMultiRad(gridProperties, pointProperties);
        }
    }
}