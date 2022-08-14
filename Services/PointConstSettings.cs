using dd_andromeda_poisson_disk_sampling.Propereties;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public class PointConstSettings : IPointSettings
    {
        public float Margin
        {
            get => _margin; 
            set => SetMargin(value);
            
        }
        
        public float Radius;

        private float _margin;
        public PointConstSettings(float radius)
        {
            Radius = radius;
        }

        public float GetPointRadius(int currentTry) => Radius + Mathf.Max(0, Margin);

        public int GetSearchSize(float pointRadius, in GridProperties gridProperties) => 3;
        
        private void SetMargin(float margin)
        {
            Radius += Mathf.Max(0, margin);
            
            _margin = margin;
        }
    }
}