using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class AbstractSingleGrid : AbstractGrid<IGrid, Point>, IGrid
    {
        protected AbstractSingleGrid(GridProperties gridProperties, ICandidateValidator<IGrid, Point> candidateValidator, IPointSettings pointSettings) : base(gridProperties, candidateValidator, pointSettings)
        {
        }

        protected override bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, 
            float candidateRadius, Vector2Int candidateCell)
        {
            return CandidateValidator.IsCandidateValid(this, searchSize, candidateWorldPosition, candidateRadius,
                candidateCell);
        }
    }
}