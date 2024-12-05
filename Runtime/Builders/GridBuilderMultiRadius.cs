using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Points;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class GridBuilderMultiRadius : GridBuilder<PointPropertiesMultiRadius, GridBuilderMultiRadius>
    {
        public IDPSGrid Build()
        {
            var gridProperties = new GridProperties(
                PointProperties.RadiusSelector.MinRadius,
                PointProperties.RadiusSelector.MinPointMargin)
            {
                FillCellsInsidePoint = true
            };
            
            GridConsumer?.Invoke(gridProperties);

            return new DPSGrid(
                IsGridUnlimited ? new CellHolderDictionary(gridProperties) : new CellHolderArray(gridProperties),
                CandidateValidator ?? new DefaultCandidateValidator(),
                gridProperties, PointProperties.RadiusSelector);
        }
    }
    
    public class GridBuilderMultiRadius<T> : GridBuilder<PointPropertiesMultiRadius, GridBuilderMultiRadius>
        where T : DPSPoint, new()
    {
        public IDPSGrid<T> Build()
        {
            var gridProperties = new GridProperties(
                PointProperties.RadiusSelector.MinRadius,
                PointProperties.RadiusSelector.MinPointMargin)
            {
                FillCellsInsidePoint = true
            };
            
            GridConsumer?.Invoke(gridProperties);

            return new DPSGrid<T>(
                IsGridUnlimited ? new CellHolderDictionary(gridProperties) : new CellHolderArray(gridProperties),
                CandidateValidator ?? new DefaultCandidateValidator(),
                gridProperties, PointProperties.RadiusSelector);
        }
    }
}