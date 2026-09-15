using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GoogleSheetDictionaryTableAttribute : GoogleSheetBindingAttribute
{
    public GoogleSheetDictionaryTableAttribute(string key = null) : base(key) { }

    public override bool Apply(object target, MemberInfo member, IReadOnlyDictionary<string, List<string>> sheetData)
    {
        string keyName = ResolveKey(member);

        // Check key with or without '#' prefix
        if (!sheetData.TryGetValue(keyName, out var keyRows) && !sheetData.TryGetValue("#" + keyName, out keyRows))
        {
            Debug.LogWarning($"[GoogleSheetDictionaryTable] Key '{keyName}' not found in sheet data. " + $"Available keys: {string.Join(", ", sheetData.Keys)}");
            return false;
        }

        if (keyRows == null || keyRows.Count == 0)
        {
            Debug.LogWarning($"[GoogleSheetDictionaryTable] Key '{keyName}' has no row data.");
            return false;
        }

        Type memberType = GetMemberType(member);
        if (memberType == null) return false;

        // Resolve dictionary TKey and TValue
        Type dictKeyType = null;
        Type dictValType = null;

        // Check generic arguments on type or base types
        Type currentType = memberType;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericArguments().Length >= 2)
            {
                Type[] args = currentType.GetGenericArguments();
                dictKeyType = args[0];
                dictValType = args[1];
                break;
            }
            currentType = currentType.BaseType;
        }

        // Check IDictionary<,> interface fallback
        if (dictKeyType == null)
        {
            foreach (Type iface in memberType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    Type[] args = iface.GetGenericArguments();
                    dictKeyType = args[0];
                    dictValType = args[1];
                    break;
                }
            }
        }

        if (dictKeyType == null || dictValType == null)
        {
            Debug.LogError($"[GoogleSheetDictionaryTable] Could not resolve Key/Value types for '{member.Name}'.");
            return false;
        }

        // Get or instantiate dictionary instance
        object dictInstance = GetMemberValue(target, member);
        if (dictInstance == null)
            dictInstance = Activator.CreateInstance(memberType);

        // Clear existing dictionary
        MethodInfo clearMethod = memberType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
        clearMethod?.Invoke(dictInstance, null);

        // Cache writable fields and properties on the Value type
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var valFields = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in dictValType.GetFields(flags))
            valFields[f.Name] = f;

        var valProps = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in dictValType.GetProperties(flags))
            if (p.CanWrite) valProps[p.Name] = p;

        // Add method / indexer lookup
        MethodInfo addMethod = memberType.GetMethod("Add", new[] { dictKeyType, dictValType });
        PropertyInfo indexerProp = memberType.GetProperty("Item", new[] { dictKeyType });

        int rowCount = keyRows.Count;

        for (int i = 0; i < rowCount; i++)
        {
            object rowKey = GoogleSheetConverter.ConvertValue(keyRows[i], dictKeyType);
            object rowVal = Activator.CreateInstance(dictValType);

            // Populate fields
            foreach (var kvp in valFields)
            {
                if (TryGetColumnData(sheetData, kvp.Key, out var colValues) && i < colValues.Count)
                {
                    object converted = GoogleSheetConverter.ConvertValue(colValues[i], kvp.Value.FieldType);
                    kvp.Value.SetValue(rowVal, converted);
                }
            }

            // Populate properties
            foreach (var kvp in valProps)
            {
                if (TryGetColumnData(sheetData, kvp.Key, out var colValues) && i < colValues.Count)
                {
                    object converted = GoogleSheetConverter.ConvertValue(colValues[i], kvp.Value.PropertyType);
                    kvp.Value.SetValue(rowVal, converted);
                }
            }

            // Assign into dictionary
            if (dictInstance is IDictionary nonGenericDict)
                nonGenericDict[rowKey] = rowVal;
            else if (indexerProp != null && indexerProp.CanWrite)
                indexerProp.SetValue(dictInstance, rowVal, new[] { rowKey });
            else if (addMethod != null)
                addMethod.Invoke(dictInstance, new[] { rowKey, rowVal });
        }

        // Force Unity SerializedDictionary to sync internal serialized lists for Inspector
        if (dictInstance is ISerializationCallbackReceiver receiver)
            receiver.OnBeforeSerialize();

        SetMemberValue(target, member, dictInstance);
        return true;
    }

    static bool TryGetColumnData(IReadOnlyDictionary<string, List<string>> sheetData, string name, out List<string> values)
    {
        return sheetData.TryGetValue(name, out values) || sheetData.TryGetValue("!" + name, out values);
    }
}