using System;
using System.Collections.Generic;
using System.Reflection;

public static class ReflectionUtils
{
    public static bool IsListOfType<T>(this FieldInfo field)
    {
        var fieldType = field.FieldType;
        return fieldType.IsGenericType &&
               fieldType.GetGenericTypeDefinition() == typeof(List<>) &&
               fieldType.GetGenericArguments()[0] == typeof(T);
    }
}