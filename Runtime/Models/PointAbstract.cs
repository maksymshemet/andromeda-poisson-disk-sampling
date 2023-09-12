using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public abstract class PointAbstract
    {
        public float FullRadius => Radius + Margin;
        
        public readonly Vector3 WorldPosition;
        public readonly float Radius;
        public readonly float Margin;

        protected PointAbstract(Vector3 worldPosition, float radius, float margin)
        {
            WorldPosition = worldPosition;
            Radius = radius;
            Margin = margin;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is PointGrid point)
            {
                return WorldPosition.Equals(point.WorldPosition) 
                       && Radius.Equals(point.Radius) 
                       && Margin.Equals(point.Margin);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (WorldPosition, Radius, Margin).GetHashCode();
        }
    }
}