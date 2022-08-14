using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public interface ICandidateValidator
    {
        bool IsCandidateValid(in IGrid grid,
            int searchSize, Vector3 candidateWorldPosition, float candidateRadius, Vector2Int candidateCell, 
            in IWorld world = null, Vector2Int chunkPosition = default);
    }

    public class CandidateValidatorGridWorld : ICandidateValidator
    {
        public bool IsCandidateValid(in IGrid grid, int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
            Vector2Int candidateCell, in IWorld world = null, Vector2Int chunkPosition = default)
        {

            if (candidateWorldPosition.x < -1.68 && candidateWorldPosition.x > -1.72 &&
                candidateWorldPosition.y < 4.5 && candidateWorldPosition.y > 4.3)
            {
                int a = 1;
            }
            
            var regionCoordinates = RegionCoordinates.Create(searchSize, candidateCell);
            for (var y = regionCoordinates.StartY; y <= regionCoordinates.EndY; y++)
            {
                for (var x = regionCoordinates.StartX; x <= regionCoordinates.EndX; x++)
                {
                    if (CellCoordInGridBounds(x, y, grid.GridProperties.CellHeight,
                            grid.GridProperties.CellWidth))
                    {
                        var point = grid.GetPointValue(x, y);
                        if(point == null) continue;
                        
                        if (IsCandidateIntersectWithPoint(candidateWorldPosition,candidateRadius, point))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var point = world.GetPoint(chunkPosition, x, y);
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

    public class CandidateValidatorGrid : ICandidateValidator
    {
        public bool IsCandidateValid(in IGrid grid, int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
            Vector2Int candidateCell, in IWorld world = null,  Vector2Int chunkPosition = default)
        {
            var regionCoordinates = RegionCoordinates.Create(
                searchSize:searchSize, 
                sourceCoordinate: candidateCell,
                startXLimit: 0, endXLimit: grid.GridProperties.CellWidth,
                startYLimit: 0, endYLimit: grid.GridProperties.CellHeight);
            
            var prevHorizontal = -1;

            for (var y = regionCoordinates.StartY; y < regionCoordinates.EndY; y++)
            {
                var coordStart = grid.FlatCoordinates(regionCoordinates.StartX, y);
                var coordEnd =  grid.FlatCoordinates(regionCoordinates.EndX, y);
                for (var x = coordStart; x < coordEnd; x++)
                {
                    var pointIndex = grid.GetCellValue(x);
                    if(pointIndex == 0 || pointIndex == prevHorizontal) continue;
                    prevHorizontal = pointIndex;
                    
                    var existingPoint = grid.GetPointValue(pointIndex - 1);
                    if (IsCandidateIntersectWithPoint(candidateWorldPosition,candidateRadius, existingPoint))
                    {
                        return false;
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
    }
}