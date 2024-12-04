namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors.MultiRadiiSelectors
{
    public interface IRadiusSelectorMulti : IRadiusSelector
    {
        float MinRadius { get; }
        float MinPointMargin { get; }
    }
}