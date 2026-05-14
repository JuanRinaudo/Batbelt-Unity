using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if NEWTONSOFT_ENABLED
using Newtonsoft.Json;
#endif

public static class ObjectExtensions
{
    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> items;
    }
    
    public static object DeepClone(this object obj)
    {
        if (obj == null) return null;
    
        var type = obj.GetType();
    
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var json = JsonUtility.ToJson(new { items = obj });
            var wrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
            var wrapper = JsonUtility.FromJson(json, wrapperType);
            return wrapperType.GetField("items").GetValue(wrapper);
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            var wrapperType = typeof(ListWrapper<>).MakeGenericType(elementType);
            var wrapper = Activator.CreateInstance(wrapperType);
            wrapperType.GetField("items").SetValue(wrapper, obj);
            var json = JsonUtility.ToJson(wrapper);
            var clonedWrapper = JsonUtility.FromJson(json, wrapperType);
            return wrapperType.GetField("items").GetValue(clonedWrapper);
        }
        else
        {
            var json = JsonUtility.ToJson(obj);
            return JsonUtility.FromJson(json, type);
        }
    }
    
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
    
    public static void DeepCopy<T>(this T self, T target)
    {
#if NEWTONSOFT_ENABLED
        var serialized = JsonConvert.SerializeObject(target);
        return JsonConvert.OverrideObject<T>(serialized);
#else
        var serialized = JsonUtility.ToJson(target);
        JsonUtility.FromJsonOverwrite(serialized, self);
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