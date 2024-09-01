using UnityEngine;

public class BatCore
{
    
    public static void Log(string message)
    {
#if DEBUG_ON
        Debug.Log(message);
#endif
    }
    
    public static void LogWarning(string message)
    {
#if DEBUG_ON
        Debug.LogWarning(message);
#endif
    }
    
    public static void LogError(string message)
    {
#if DEBUG_ON
        Debug.LogError(message);
#endif
    }

}