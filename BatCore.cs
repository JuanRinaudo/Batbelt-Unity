using UnityEngine;

public class BatCore
{
    
    public static void Log(string message)
    {
        Debug.Log(message);
    }
    
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }
    
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }

}