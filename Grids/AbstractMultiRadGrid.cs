using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class AbstractMultiRadGrid : AbstractGrid
    {
        public float[] RadiusChangePercents { get; set; }
        public IFillPoints PointFiller { get; set; }
        
        public readonly float MinRadius;
        public readonly float MaxRadius;
        
        protected AbstractMultiRadGrid(GridProperties gridProperties, float minRadius, float maxRadius)
            : base(gridProperties)
        {
            MinRadius = minRadius;
            MaxRadius = maxRadius;
        }
        
        protected override float GetPointRadius(int currentTry)
        {
            var radius = Random.Range(MinRadius, MaxRadius);
            if (currentTry == 0 || RadiusChangePercents == null)
            {
                return radius;
            }
            
            radius *= RadiusChangePercents[Mathf.Min(currentTry, RadiusChangePercents.Length - 1)];
            return Mathf.Min(MaxRadius, Mathf.Max(MinRadius, radius));
        }

        protected override int GetSearchSize(float pointRadius) => 
            Mathf.Max(3, Mathf.CeilToInt(pointRadius / GridProperties.CellSize));

        protected override void OnPointCreated(ref Point point)
        {
            PointFiller?.FillPoints( this, point);
        }
    }
}