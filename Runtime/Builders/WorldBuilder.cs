using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class WorldSizeBuilder
    {
        public Func<float, Vector2Int> WithCellsCount(int chunkCellsCount)
        {
            return _ => new Vector2Int(chunkCellsCount, chunkCellsCount);
        }
        
        public Func<float, Vector2Int> WithCellsCount(int chunkCellsCountX, int chunkCellsCountY)
        {
            return _ => new Vector2Int(chunkCellsCountX, chunkCellsCountY);
        }
        
        public Func<float, Vector2Int> WithApproximateSize(Vector2 approximateSize)
        {
            return cellSize =>
            {
                int x = Mathf.RoundToInt(approximateSize.x / cellSize);
                int y = Mathf.RoundToInt(approximateSize.y / cellSize);
                return new Vector2Int(x, y);
            };
        }
    }

    public abstract class WorldBuilder<TPointProperties, TPointPropertiesBuilder, TWorld, TSelf> : IBuilder<TWorld>
        where TPointProperties : PointProperties
        where TPointPropertiesBuilder : PointPropertiesBuilder<TPointProperties, TPointPropertiesBuilder>, new()
        where TSelf : WorldBuilder<TPointProperties, TPointPropertiesBuilder, TWorld, TSelf>
    {
        protected TPointPropertiesBuilder PointPropertiesBuilder;
        protected GridPropertiesBuilderForWorld GridPropertiesBuilder;

        private Func<float, Vector2Int> _sizeBuilderFunc;
        
        public TSelf WithChunkSize(Func<WorldSizeBuilder, Func<float, Vector2Int>> cells)
        {
            _sizeBuilderFunc = cells(new WorldSizeBuilder());
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
            Vector2Int cellLenght = _sizeBuilderFunc(cellSize);
            var size = new Vector2(
                x: cellSize * cellLenght.x,
                y: cellSize * cellLenght.y);

            gridProperties.Size = size;
            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtX = cellLenght.x;
            gridProperties.CellLenghtY = cellLenght.y;
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