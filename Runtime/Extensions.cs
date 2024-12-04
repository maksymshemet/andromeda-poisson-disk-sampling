using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime
{
    public static class Extensions
    {
        internal static PointSize Closest(this PointSize []arr,
            float target)
        {
            int n = arr.Length;
 
            // Corner cases
            if (target <= (arr[0].Radius + arr[0].Margin))
                return arr[0];
            if (target >= (arr[n - 1].Radius + arr[n - 1].Margin))
                return arr[n - 1];
 
            // Doing binary search
            int i = 0, j = n, mid = 0;
            while (i < j)
            {
                mid = (i + j) / 2;
 
                if (Mathf.Approximately(arr[mid].Radius + arr[mid].Margin, target))
                    return arr[mid];
 
                /* If target is less
                than array element,
                then search in left */
                if (target < arr[mid].Radius)
                {
         
                    // If target is greater
                    // than previous to mid,
                    // return closest of two
                    if (mid > 0 && target > (arr[mid - 1].Radius + arr[mid - 1].Margin))
                        return GetClosest(arr[mid - 1],
                            arr[mid], target);
                 
                    /* Repeat for left half */
                    j = mid;            
                }
 
                // If target is
                // greater than mid
                else
                {
                    if (mid < n-1 && target < (arr[mid + 1].Radius + arr[mid + 1].Margin))
                        return GetClosest(arr[mid],
                            arr[mid + 1], target);        
                    i = mid + 1; // update i
                }
            }
 
            // Only single element
            // left after search
            return arr[mid];
        }
        
        private static PointSize GetClosest(PointSize val1, PointSize val2, float target) =>
            target - (val1.Radius + val1.Margin) >= (val2.Radius + val2.Margin) - target ? val2 : val1;
    }
}