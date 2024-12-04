using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Points;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class GridBuilderMultiRadius : GridBuilder<PointPropertiesMultiRadius, GridBuilderMultiRadius>
    {
        public override IGrid Build()
        {
            var gridProperties = new GridProperties(
                PointProperties.RadiusSelector.MinRadius,
                PointProperties.RadiusSelector.MinPointMargin)
            {
                FillCellsInsidePoint = true
            };
            
            GridConsumer?.Invoke(gridProperties);

            return new PointsHolder(
                IsGridUnlimited ? new CellHolderDictionary(gridProperties) : new CellHolderArray(gridProperties),
                CandidateValidator ?? new DefaultCandidateValidator(),
                gridProperties, PointProperties.RadiusSelector);
        }
    }
}