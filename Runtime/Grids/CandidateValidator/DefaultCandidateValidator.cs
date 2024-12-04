using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator
{
    public class DefaultCandidateValidator : ICandidateValidator
    {
        public virtual bool IsValid(IGrid grid, Candidate candidate, int searchSize)
        {
            Vector2Int cellMin = grid.Cells.CellFromWorldPosition(candidate.WorldPosition,
                WorldToCellPositionMethod.Floor);
            Vector2Int cellMax = grid.Cells.CellFromWorldPosition(candidate.WorldPosition,
                WorldToCellPositionMethod.Ceil);
            
            SearchBoundaries searchBoundaries = Helper
                .GetSearchBoundaries(grid, cellMin, cellMax, searchSize);
            
            int startY = searchBoundaries.StartY;
            int endY = searchBoundaries.EndY;

            while (startY <= endY && endY >= startY)
            {
                int startX = searchBoundaries.StartX;
                int endX = searchBoundaries.EndX;

                while (startX <= endX && endX >= startX)
                {
                    if (
                        IsIntersect(startX, startY)
                        || IsIntersect(startX, endY)
                        || IsIntersect(endX, startY)
                        || IsIntersect(endX, endY)
                        )
                    {
                        return false;
                    }
                    
                    startX++;
                    endX--;
                }
                
                startY++;
                endY--;
            }

            return true;

            bool IsIntersect(int x, int y)
            {
                int pointIndex = grid.Cells.GetCellValue(x, y);
                if (pointIndex == 0) return false;

                Point existingPoint = grid.GetPointByIndex(pointIndex);
                return candidate.IsIntersectWithPoint(existingPoint);
            }
        }
    }
}