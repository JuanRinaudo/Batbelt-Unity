using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public static class GoogleSheetConverter
{
    public static object ConvertValue(string raw, Type targetType)
    {
        if (raw == null) return null;
        raw = raw.Trim();

        if (targetType == typeof(string)) return raw;

        if (targetType == typeof(float))
        {
            return float.Parse(raw.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(double))
        {
            return double.Parse(raw.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(int))
        {
            return int.Parse(raw, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(long))
        {
            return long.Parse(raw, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(raw, out bool b)) return b;
            if (raw == "1") return true;
            if (raw == "0") return false;
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, raw, true);
        }

        return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
    }

    public static object CreateCollection(Type targetType, List<string> rawList)
    {
        if (targetType.IsArray)
        {
            Type elementType = targetType.GetElementType();
            Array array = Array.CreateInstance(elementType, rawList.Count);
            for (int i = 0; i < rawList.Count; i++)
            {
                array.SetValue(ConvertValue(rawList[i], elementType), i);
            }
            return array;
        }

        if (typeof(IList).IsAssignableFrom(targetType) &&
            targetType.IsGenericType)
        {
            Type elementType = targetType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(targetType);
            for (int i = 0; i < rawList.Count; i++)
            {
                list.Add(ConvertValue(rawList[i], elementType));
            }
            return list;
        }

        return null;
    }
}