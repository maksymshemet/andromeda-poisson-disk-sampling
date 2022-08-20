using dd_andromeda_poisson_disk_sampling.Propereties;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class PointMultiRadSettings : IPointSettings
    {
        public float Margin
        {
            get => _margin; 
            set => SetMargin(value);
            
        }
        
        public float[] RadiusChangePercents { get; set; }
        
        public float MinRadius;
        public float MaxRadius;

        private float _margin;
        
        public PointMultiRadSettings(float minRadius, float maxRadius)
        {
            MaxRadius = maxRadius;
            MinRadius = minRadius;
        }

        private void SetMargin(float margin)
        {
            MinRadius += Mathf.Max(0, margin);
            MaxRadius += Mathf.Max(0, margin);

            _margin = margin;
        }
        
        public float GetPointRadius(int currentTry)
        {
            var radius = Random.Range(MinRadius, MaxRadius);
            if (currentTry == 0 || RadiusChangePercents == null)
            {
                return radius;
            }
            
            radius *= RadiusChangePercents[Mathf.Min(currentTry, RadiusChangePercents.Length - 1)];
            return Mathf.Min(MaxRadius, Mathf.Max(MinRadius, radius));
        }

        public int GetSearchSize(float pointRadius, in GridProperties gridProperties)
        {
            return Mathf.Max(3, Mathf.CeilToInt(pointRadius / gridProperties.CellSize));
        }
    }
}