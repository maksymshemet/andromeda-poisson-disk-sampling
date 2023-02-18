using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public abstract class PointAbstract
    {
        public float FullRadius => Radius + Margin;
        
        public Vector3 WorldPosition;
        public float Radius;
        public float Margin;

        public override bool Equals(object obj)
        {
            if (obj is PointGrid point)
            {
                return WorldPosition.Equals(point.WorldPosition) && Radius.Equals(point.Radius);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (WorldPosition, Radius).GetHashCode();
        }
    }
}