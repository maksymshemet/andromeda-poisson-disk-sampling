using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;

namespace dd_andromeda_poisson_disk_sampling
{
    public class MultiRadGrid : AbstractSingleGrid
    {
        public IFillPoints<IGrid, Point> PointFiller { get; set; }

        public MultiRadGrid(GridProperties gridProperties, ICandidateValidator<IGrid, Point> candidateValidator, IPointSettings pointSettings) : base(gridProperties, candidateValidator, pointSettings)
        {
        }

        protected override void PostPointCreated(in Point point, int pointIndex)
        {
            base.PostPointCreated(point, pointIndex);
            
            PointFiller?.FillPoints( this, point);
        }
    }
}