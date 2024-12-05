using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator
{
    public class DefaultCandidateValidator : ICandidateValidator
    {
        public virtual bool IsValid(IDPSGridConfig gridConfig, Candidate candidate, int searchSize)
        {
            Vector2Int cellMin = gridConfig.Cells.CellFromWorldPosition(candidate.WorldPosition,
                WorldToCellPositionMethod.Floor);
            Vector2Int cellMax = gridConfig.Cells.CellFromWorldPosition(candidate.WorldPosition,
                WorldToCellPositionMethod.Ceil);
            
            SearchBoundaries searchBoundaries = Helper
                .GetSearchBoundaries(gridConfig, cellMin, cellMax, searchSize);
            
            int startY = searchBoundaries.StartY;
            int endY = searchBoundaries.EndY;

            while (startY <= endY && endY >= startY)
            {
                int startX = searchBoundaries.StartX;
                int endX = searchBoundaries.EndX;

                while (startX <= endX && endX >= startX)
                {
                    if (
                        gridConfig.IsPositionFree(candidate, startX, startY)
                        || gridConfig.IsPositionFree(candidate,startX, endY)
                        || gridConfig.IsPositionFree(candidate,endX, startY)
                        || gridConfig.IsPositionFree(candidate,endX, endY)
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
        }
    }
}