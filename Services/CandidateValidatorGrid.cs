using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class CandidateValidatorGrid : ICandidateValidator<IGrid, Point>
    {
        public bool IsCandidateValid(in IGrid grid, int searchSize, Vector3 candidateWorldPosition, float candidateRadius,
            Vector2Int candidateCell)
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
                    
                    var existingPoint = grid.GetPoint(pointIndex - 1);
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