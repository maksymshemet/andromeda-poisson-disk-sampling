using System;
using System.Reflection;

namespace dd_andromeda_poisson_disk_sampling
{
    public static class Helper
    {
        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
 
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
 
            clearMethod.Invoke(null, null);
        }
    }
}