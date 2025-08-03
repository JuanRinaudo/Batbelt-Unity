using UnityEngine;

#if NEWTONSOFT_ENABLED
using Newtonsoft.Json;

public static class ObjectExtensions
{
    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonConvert.SerializeObject(self);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}
#else
public static class ObjectExtensions
{
    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonUtility.ToJson(self);
        return JsonUtility.FromJson<T>(serialized);
    }
}
#endif