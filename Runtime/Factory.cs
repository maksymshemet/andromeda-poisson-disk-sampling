using System.Linq;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using dd_andromeda_poisson_disk_sampling.Services;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public static class Factory
    {
        private const float Sqrt2 = 1.41421354f;
        
        public static IGrid CreateGrid(
            Vector2Int size, 
            float minRadius, 
            float maxRadius, 
            int tries = 20,
            AnimationCurve radiusChangeCurve = null)
        {
            var cellSize = minRadius / Sqrt2;
            var cellWidth =  Mathf.CeilToInt(size.x / cellSize);
            var cellHeight = Mathf.CeilToInt(size.y / cellSize);
            float[] radiusChangePercents = null;
            
            if (radiusChangeCurve != null)
            {
                radiusChangePercents = new float[tries];
                for (var i = 0; i < tries; i++)
                {
                    radiusChangePercents[i] = radiusChangeCurve.Evaluate((float)i / tries);
                }
            }
            var gridProperties = new GridProperties
            {
                Size = size,
                CellSize = cellSize,
                CellHeight = cellHeight,
                CellWidth = cellWidth,
                Tries = tries
            };
            var ps = new PointMultiRadSettings(minRadius, maxRadius)
            {
                RadiusChangePercents = radiusChangePercents
            };
            
            return new MultiRadGrid(gridProperties, new CandidateValidatorGrid(), ps)
            {
                PointFiller = new FillGridPoints(),
            };
        }
        
        public static IGrid CreateGrid(
            Vector2Int size, 
            float radius, 
            int tries = 20)
        {
            var cellSize = radius / Sqrt2;
            var cellWidth =  Mathf.CeilToInt(size.x / cellSize);
            var cellHeight = Mathf.CeilToInt(size.y / cellSize);
            var gridProperties = new GridProperties
            {
                Size = size,
                CellSize = cellSize,
                CellHeight = cellHeight,
                CellWidth = cellWidth,
                Tries = tries
            };

            var ps = new PointConstSettings(radius);
            
            return new StaticGrid(gridProperties, new CandidateValidatorGrid(), ps)
            {
            };
        }

        public static World CreateWorld(Vector2Int size,
            float radius,
            int tries = 20,
            float pointMargin = 0,
            AnimationCurve radiusChangeCurve = null,
            Vector3 worldPositionOffset = default)
        {
            var cellSize = (radius + pointMargin) / Sqrt2;
            var cellWidth =  Mathf.FloorToInt(size.x / cellSize);
            var cellHeight = Mathf.FloorToInt(size.y / cellSize);
            
            var chunkSize = new Vector2(
                x:cellSize * cellWidth,
                y:cellSize * cellHeight);
            
            var gridProperties = new GridProperties
            {
                CellHeight = cellHeight,
                CellSize = cellSize,
                CellWidth = cellWidth,
                Size = chunkSize,
                Tries = tries,
            };
        
            var r = new RadiusConst(radius + pointMargin);
            
            return new World(radius:r, worldPositionOffset: worldPositionOffset, gridProperties: gridProperties,
                gridFactory: CreateSingleRadGrid)
            {
                Tries = tries,
                Margin = pointMargin
            };
        }
        public static World CreateWorld(Vector2Int size,
            float minRadius,
            float maxRadius,
            int tries = 20,
            float pointMargin = 0,
            AnimationCurve radiusChangeCurve = null,
            Vector3 worldPositionOffset = default)
        {
            var cellSize = (minRadius + pointMargin) / Sqrt2;
            var cellWidth =  Mathf.FloorToInt(size.x / cellSize);
            var cellHeight = Mathf.FloorToInt(size.y / cellSize);
            
            float[] radiusChangePercents = null;
            
            if (radiusChangeCurve != null)
            {
                radiusChangePercents = new float[tries];
                for (var i = 0; i < tries; i++)
                {
                    radiusChangePercents[i] = radiusChangeCurve.Evaluate((float) i / tries);
                }
            }

            var chunkSize = new Vector2(
                x:cellSize * cellWidth,
                y:cellSize * cellHeight);
            
            var gridProperties = new GridProperties
            {
                CellHeight = cellHeight,
                CellSize = cellSize,
                CellWidth = cellWidth,
                Size = chunkSize,
                Tries = tries,
            };

            var radius = new RadiusMinMax(minRadius + pointMargin, maxRadius + pointMargin, radiusChangePercents);
            return new World(radius:radius, worldPositionOffset: worldPositionOffset, gridProperties: gridProperties,
                gridFactory: CreateMultiRadGrid)
            {
                Tries = tries,
                Margin = pointMargin
            };
        }
        
        public static World CreateWorld(Vector2Int size,
            float[] radiuses,
            int tries = 20,
            float pointMargin = 0,
            AnimationCurve radiusChangeCurve = null,
            Vector3 worldPositionOffset = default)
        {
            var cellSize = (radiuses.Min() + pointMargin) / Sqrt2;
            var cellWidth =  Mathf.FloorToInt(size.x / cellSize);
            var cellHeight = Mathf.FloorToInt(size.y / cellSize);
            
            float[] radiusChangePercents = null;
            
            if (radiusChangeCurve != null)
            {
                radiusChangePercents = new float[tries];
                for (var i = 0; i < tries; i++)
                {
                    radiusChangePercents[i] = radiusChangeCurve.Evaluate((float) i / tries);
                }
            }

            var chunkSize = new Vector2(
                x:cellSize * cellWidth,
                y:cellSize * cellHeight);
            
            var gridProperties = new GridProperties
            {
                CellHeight = cellHeight,
                CellSize = cellSize,
                CellWidth = cellWidth,
                Size = chunkSize,
                Tries = tries,
            };

            if (pointMargin != 0)
            {
                for (var i = 0; i < radiuses.Length; i++)
                {
                    radiuses[i] += pointMargin;
                }
            }
            
            var radius = new RadiusPredefined(radiuses, radiusChangePercents);
            return new World(radius:radius, worldPositionOffset: worldPositionOffset, gridProperties: gridProperties,
                gridFactory: CreateMultiRadGrid)
            {
                Tries = tries,
                Margin = pointMargin
            };
        }
        
        private static GridWorld CreateMultiRadGrid(GridCore core, World world, Vector2Int chunkPosition)
        {
            return new GridWorldMultiRad(gridCore: core, world: world,
                chunkPosition: chunkPosition);
        }
        
        private static GridWorld CreateSingleRadGrid(GridCore core, World world, Vector2Int chunkPosition)
        {
            return new GridWorldSingleRad(gridCore: core, world: world,
                chunkPosition: chunkPosition);
        }
        // internal static GridWorld CreateWorldGrid(World world, Vector2Int chunkPosition)
        // {
        //     float cx = (chunkPosition.x * world.GridProperties.Size.x) + world.WorldPositionOffset.x;
        //     float cy = (chunkPosition.y * world.GridProperties.Size.y) + world.WorldPositionOffset.y;
        //
        //     var core = new GridCore(world.GridProperties)
        //     {
        //         WorldPositionOffset = new Vector3(cx, cy)
        //     };
        //     
        //     return new GridWorldMultiRad(gridCore: core, world: world, 
        //         chunkPosition: chunkPosition)
        //     {
        //     };
        // }
    }
}