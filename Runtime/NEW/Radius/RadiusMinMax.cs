using System.Collections.Generic;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties.Radius
{
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
    
    public class RadiusPredefined : IRadius
    {
        private readonly float[] _radiuses;
        private readonly float[] _radiusChangePercents;

        public RadiusPredefined(float[] radiuses, float[] radiusChangePercents = null)
        {
            _radiuses = radiuses;
            _radiusChangePercents = radiusChangePercents;
        }

        public float GetRadius(int currentTry, int maxTries)
        {
            var radius = _radiuses[Random.Range(0, _radiuses.Length)];
            if (currentTry == 0 || _radiusChangePercents == null)
            {
                return radius;
            }
            
            radius *= _radiusChangePercents[Mathf.Min(currentTry, _radiusChangePercents.Length - 1)];
            return ClosestTo(_radiuses, radius);
        }
        
        public static float ClosestTo(IEnumerable<float> collection, float target)
        {
            // NB Method will return int.MaxValue for a sequence containing no elements.
            // Apply any defensive coding here as necessary.
            var closest = float.MaxValue;
            var minDifference = float.MaxValue;
            foreach (var element in collection)
            {
                var difference = Mathf.Abs((long)element - target);
                if (minDifference > difference)
                {
                    minDifference = (int)difference;
                    closest = element;
                }
            }

            return closest;
        }
    }
}