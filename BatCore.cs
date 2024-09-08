using UnityEngine;

public class BatCore
{
    
    public static void Log(string message)
    {
#if DEBUG_ON || UNITY_EDITOR
        Debug.Log(message);
#endif
    }
    
    public static void LogWarning(string message)
    {
#if DEBUG_ON || UNITY_EDITOR
        Debug.LogWarning(message);
#endif
    }
    
    public static void LogError(string message)
    {
#if DEBUG_ON || UNITY_EDITOR
        Debug.LogError(message);
#endif
    }

}