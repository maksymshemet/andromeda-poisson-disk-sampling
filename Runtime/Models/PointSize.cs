namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models 
{
    public class PointSize
    {
        public float Radius;
        public float Margin;

        public override bool Equals(object obj)
        {
            if (obj is PointSize size)
            {
                return Radius.Equals(size.Radius)
                       && Margin.Equals(size.Margin);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Radius, Margin).GetHashCode();
        }
    }
}