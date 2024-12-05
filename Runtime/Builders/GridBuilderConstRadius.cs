using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Points;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class GridBuilderConstRadius : GridBuilder<PointPropertiesConstRadius, GridBuilderConstRadius>
    {
        public IDPSGrid Build()
        {
            var gridProperties = new GridProperties(
                PointProperties.Radius,
                PointProperties.PointMargin);
            GridConsumer?.Invoke(gridProperties);

            return new DPSGrid(
                IsGridUnlimited ? new CellHolderDictionary(gridProperties) : new CellHolderArray(gridProperties),
                CandidateValidator ?? new DefaultCandidateValidator(),
                gridProperties, new RadiusSelectorConst
                {
                    Radius = PointProperties.Radius,
                    PointMargin = PointProperties.PointMargin
                });
        }
    }
    
    public class GridBuilderConstRadius<T> : GridBuilder<PointPropertiesConstRadius, GridBuilderConstRadius<T>> 
        where T : DPSPoint, new()
    {
        public IDPSGrid<T> Build()
        {
            var gridProperties = new GridProperties(
                PointProperties.Radius,
                PointProperties.PointMargin);
            GridConsumer?.Invoke(gridProperties);

            return new DPSGrid<T>(
                IsGridUnlimited ? new CellHolderDictionary(gridProperties) : new CellHolderArray(gridProperties),
                CandidateValidator ?? new DefaultCandidateValidator(),
                gridProperties, new RadiusSelectorConst
                {
                    Radius = PointProperties.Radius,
                    PointMargin = PointProperties.PointMargin
                });
        }
    }
}