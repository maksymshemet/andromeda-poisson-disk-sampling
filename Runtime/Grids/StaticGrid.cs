using dd_andromeda_poisson_disk_sampling;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;

public class StaticGrid : AbstractSingleGrid
{
    public StaticGrid(GridProperties gridProperties, ICandidateValidator<IGrid, Point> candidateValidator, IPointSettings pointSettings) : base(gridProperties, candidateValidator, pointSettings)
    {
    }
}