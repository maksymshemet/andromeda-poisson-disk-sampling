using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Demo
{
    public class DemoStepRadPoint : MonoBehaviour
    {
        public Point Point { get; set; }
        public IGrid AbstractGrid { get; set; }
        public StaticMultiRadStepDemo Demo { get; set; }

        
        [Header("Trigger")] public bool Trigger;

        private static bool CachedTrigger;

        private void OnValidate()
        {
            if (CachedTrigger != Trigger)
            {
                Demo.Spawn(Point);
                CachedTrigger = Trigger;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Point.WorldPosition, Point.Radius);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Point.WorldPosition, Point.Radius);
        }
    }
}