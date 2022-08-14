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

        protected override void OnPointCreated(in Point point)
        {
            base.OnPointCreated(point);
            
            PointFiller?.FillPoints( this, point);
        }
    }
}