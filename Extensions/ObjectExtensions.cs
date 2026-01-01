using System.Reflection;
using UnityEngine;

#if NEWTONSOFT_ENABLED
using Newtonsoft.Json;
#endif

public static class ObjectExtensions
{
    public static T DeepCopy<T>(this T self)
    {
#if NEWTONSOFT_ENABLED
        var serialized = JsonConvert.SerializeObject(self);
        return JsonConvert.DeserializeObject<T>(serialized);
#else
        var serialized = JsonUtility.ToJson(self);
        return JsonUtility.FromJson<T>(serialized);
#endif
    }
    
    public static T GetFieldValue<T>(this object obj, string name) {
        var fieldInfo = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fieldInfo != null ? (T)fieldInfo?.GetValue(obj) : default(T);
    }
    
    public static T GetFieldValueRecursive<T>(this object obj, string name)
    {
        var type = obj.GetType();
        FieldInfo fieldInfo = null;
        while (fieldInfo == null && type.BaseType != null)
        {
            type = type.BaseType;
            fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
        return fieldInfo != null ? (T)fieldInfo?.GetValue(obj) : default(T);
    }
}