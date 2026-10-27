using UnityEngine;

public static class ObjectUtils
{
    public static void SafeDestroyObject(Object obj)
    {
        if (Application.isPlaying)
            Object.Destroy(obj);
        else
            Object.DestroyImmediate(obj);
    }
}