namespace dd_andromeda_poisson_disk_sampling.Services
{
    public interface IFillPoints<T, in P> where T : IGridAbstract<P>
        where P : Point
    {
        public void FillPoints(in T grid, P point);
    }
}