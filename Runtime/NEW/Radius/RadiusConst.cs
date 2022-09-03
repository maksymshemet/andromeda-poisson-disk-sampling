namespace dd_andromeda_poisson_disk_sampling.Propereties.Radius
{
    public class RadiusConst : IRadius
    {
        private readonly float _radius;

        public RadiusConst(float radius)
        {
            _radius = radius;
        }

        public float GetRadius(int currentTry, int maxTries)
        {
            return _radius;
        }
    }
}