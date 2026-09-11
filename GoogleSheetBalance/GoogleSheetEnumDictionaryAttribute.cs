using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GoogleSheetEnumDictionaryAttribute : GoogleSheetBindingAttribute
{
    readonly char separator;

    public GoogleSheetEnumDictionaryAttribute(string key = null, char separator = '.') : base(key)
    {
        this.separator = separator;
    }

    public override bool Apply(object target, MemberInfo member, IReadOnlyDictionary<string, List<string>> sheetData)
    {
        Type memberType = GetMemberType(member);
        if (memberType == null) return false;

        if (!TryGetDictionaryTypes(memberType, out Type keyType, out Type valueType))
        {
            Debug.LogError($"[GoogleSheetEnumDictionary] {member.Name} is not a valid dictionary.");
            return false;
        }

        if (!keyType.IsEnum)
        {
            Debug.LogError($"[GoogleSheetEnumDictionary] Key type {keyType.Name} is not an Enum.");
            return false;
        }

        object dictObj = GetMemberValue(target, member);
        if (dictObj == null)
        {
            dictObj = Activator.CreateInstance(memberType);
            SetMemberValue(target, member, dictObj);
        }

        string prefix = $"{ResolveKey(member)}{separator}";
        bool modified = false;

        foreach (var (sheetKey, rawValues) in sheetData)
        {
            if (!sheetKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            string enumName = sheetKey.Substring(prefix.Length).Trim();

            if (!Enum.TryParse(keyType, enumName, true, out object enumKey))
            {
                Debug.LogWarning($"[GoogleSheetEnumDictionary] '{enumName}' is not a valid {keyType.Name}.");
                continue;
            }

            object collection = GoogleSheetConverter.CreateCollection(valueType, rawValues);
            if (collection == null)
                continue;

            SetOrUpdateEntry(dictObj, keyType, valueType, enumKey, collection);
            modified = true;
        }

        if (modified)
        {
            if (dictObj is ISerializationCallbackReceiver dictReceiver)
                dictReceiver.OnBeforeSerialize();

            if (target is ISerializationCallbackReceiver targetReceiver)
                targetReceiver.OnBeforeSerialize();

            SetMemberValue(target, member, dictObj);
        }

        return modified;
    }

    static void SetOrUpdateEntry(object dictObj, Type keyType, Type valueType, object enumKey, object newCollection)
    {
        if (dictObj is IDictionary nonGenericDict)
        {
            if (nonGenericDict.Contains(enumKey))
            {
                if (nonGenericDict[enumKey] is IList existingList && newCollection is IList newItems)
                {
                    existingList.Clear();
                    foreach (var item in newItems)
                        existingList.Add(item);
                }
                else
                {
                    nonGenericDict[enumKey] = newCollection;
                }
            }
            else
            {
                nonGenericDict[enumKey] = newCollection;
            }
            return;
        }

        Type genericDictType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
        MethodInfo containsKey = genericDictType.GetMethod("ContainsKey", new[] { keyType });
        PropertyInfo indexer = genericDictType.GetProperty("Item", new[] { keyType });
        MethodInfo addMethod = genericDictType.GetMethod("Add", new[] { keyType, valueType });

        bool keyExists = containsKey != null && (bool)containsKey.Invoke(dictObj, new[] { enumKey });
        if (keyExists && indexer != null)
        {
            object existingVal = indexer.GetValue(dictObj, new[] { enumKey });
            if (existingVal is IList existingList && newCollection is IList newItems)
            {
                existingList.Clear();
                foreach (var item in newItems)
                    existingList.Add(item);
                return;
            }

            indexer.SetValue(dictObj, newCollection, new[] { enumKey });
        }
        else
        {
            if (indexer != null)
                indexer.SetValue(dictObj, newCollection, new[] { enumKey });
            else if (addMethod != null)
                addMethod.Invoke(dictObj, new[] { enumKey, newCollection });
        }
    }

    static bool TryGetDictionaryTypes(Type type, out Type keyType, out Type valueType)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var args = iface.GetGenericArguments();
                keyType = args[0];
                valueType = args[1];
                return true;
            }
        }

        keyType = null;
        valueType = null;
        return false;
    }
}