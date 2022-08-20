using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class CandidateValidatorGridWorld : ICandidateValidator<IWorldGrid, PointWorld>
    {
        public bool IsCandidateValid(in IWorldGrid grid, int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
            Vector2Int candidateCell)
        {
            var regionCoordinates = RegionCoordinates.Create(searchSize, candidateCell);
            for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
            {
                for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                {
                    if (CellCoordInGridBounds(x, y, grid.GridProperties.CellHeight,
                            grid.GridProperties.CellWidth))
                    {
                        var point = grid.GetPoint(x, y);
                        if(point == null) continue;
                        
                        if (IsCandidateIntersectWithPoint(candidateWorldPosition,candidateRadius, point))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var point = grid.World.GetPoint(grid.ChunkPosition, x, y);
                        if(point == null) continue;
                        
                        if (IsCandidateIntersectWithPoint(candidateWorldPosition,candidateRadius, point))
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }

        private bool IsCandidateIntersectWithPoint(Vector3 candidateWorldPosition, float candidateRadius,
            Point point)
        {
            var sqrDst = (point.WorldPosition - candidateWorldPosition).sqrMagnitude;
            var radius = point.Radius + candidateRadius;
            return sqrDst < (radius * radius);
        }
        
        private bool CellCoordInGridBounds(int x, int y, int gridHeight, int gridWidth) =>
            y >= 0 && y < gridHeight && x >= 0 && x < gridWidth;
    }
}