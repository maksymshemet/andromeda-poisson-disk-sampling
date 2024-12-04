using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator
{
    public interface ICandidateValidator
    {
        public bool IsValid(IGrid grid, Candidate candidate, int searchSize);
    }
}