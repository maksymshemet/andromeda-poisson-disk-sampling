using dd_andromeda_poisson_disk_sampling;
using dd_andromeda_poisson_disk_sampling.Propereties;

public class StaticGrid : AbstractGrid
{
    public float Radius { get; }
    
    public StaticGrid(GridProperties gridProperties, float radius) : base(gridProperties)
    {
        Radius = radius;
    }

    protected override float GetPointRadius(int currentTry) => Radius;

    protected override int GetSearchSize(float pointRadius) => 3;
}