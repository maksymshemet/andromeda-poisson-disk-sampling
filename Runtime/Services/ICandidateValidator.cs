using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public interface ICandidateValidator<T, P> where T : IGridAbstract<P>
        where P : Point
    {
        bool IsCandidateValid(in T grid, int searchSize, Vector3 candidateWorldPosition, 
            float candidateRadius, Vector2Int candidateCel);
    }
}