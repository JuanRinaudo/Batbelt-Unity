using System.Collections.Generic;
using UnityEngine;

public static class EnumerableExtensions
{
    public static T RandomElement<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }
    
    public static T RandomElement<T>(this List<T> array)
    {
        return array[Random.Range(0, array.Count)];
    }
}