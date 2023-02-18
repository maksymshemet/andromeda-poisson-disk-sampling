namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public interface IBuilder<out T>
    {
        T Build();
    }
}