using System.Collections.Generic;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class PointHolder<P>  where P : Point, new()
    {
         
        public IReadOnlyList<P> Points => _points;
        
        private readonly int[] _cells;
        private readonly List<P> _points;
    }
}