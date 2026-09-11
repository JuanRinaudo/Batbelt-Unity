public static class UnityExtensions
{
    public static bool IsDestroyed(this object obj)
    {
        if (obj == null) return true;
        
        if (obj is UnityEngine.Object unityObj)
            return unityObj == null;

        return false;
    }
}