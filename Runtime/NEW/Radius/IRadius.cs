using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties.Radius
{
    public interface IRadius
    {
        public float GetRadius(int currentTry, int maxTries);
    }

    public class RadiusMinMax : IRadius
    {
        private readonly float _minRadius;
        private readonly float _maxRadius;
        private readonly float[] _radiusChangePercents;

        public RadiusMinMax(float minRadius, float maxRadius, float[] radiusChangePercents = null)
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _radiusChangePercents = radiusChangePercents;
        }

        public float GetRadius(int currentTry, int maxTries)
        {
            var radius = Random.Range(_minRadius, _maxRadius);
            if (currentTry == 0 || _radiusChangePercents == null)
            {
                return radius;
            }
            
            radius *= _radiusChangePercents[Mathf.Min(currentTry, _radiusChangePercents.Length - 1)];
            return Mathf.Min(_maxRadius, Mathf.Max(_minRadius, radius));
        }
    }
}