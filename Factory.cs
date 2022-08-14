using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
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
            
            return new MultiRadGrid(gridProperties, minRadius, maxRadius)
            {
                RadiusChangePercents = radiusChangePercents,
                PointFiller = new FillGridPoints(),
                CandidateValidator = new CandidateValidatorGrid()
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

            return new StaticGrid(gridProperties, radius)
            {
                CandidateValidator = new CandidateValidatorGrid()
            };
        }

        public static IWorld CreateWorld(Vector2Int size,
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
                    radiusChangePercents[i] = radiusChangeCurve.Evaluate((float) i / tries);
                }
            }

            var gridProperties = new GridProperties
            {
                CellHeight = cellHeight,
                CellSize = cellSize,
                CellWidth = cellWidth,
                Size = size,
                Tries = tries,
            };

            return new World(gridProperties, minRadius, maxRadius)
            {
                RadiusChangePercents = radiusChangePercents
            };
        }
        
        internal static MultiRadWorldGrid CreateWorldGrid(World world, Vector2Int chunkPosition)
        {
            float cx = chunkPosition.x * world.ChunkSize.x;
            float cy = chunkPosition.y * world.ChunkSize.y;
            
            return new MultiRadWorldGrid(world: world, chunkPosition: chunkPosition, world.MinRadius, world.MaxRadius)
            {
                WorldPositionOffset = new Vector3(cx, cy),
                RadiusChangePercents = world.RadiusChangePercents,
                PointFiller = new FillWorldGridPoints(),
                CandidateValidator = new CandidateValidatorGridWorld()
            };
        }
    }
}