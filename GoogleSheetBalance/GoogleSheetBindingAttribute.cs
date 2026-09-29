using System;
using System.Collections.Generic;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class GoogleSheetBindingAttribute : Attribute
{
    public string Key { get; }

    protected GoogleSheetBindingAttribute(string key = null)
    {
        Key = key;
    }

    public abstract bool Apply(object target, MemberInfo member, IReadOnlyDictionary<string, List<string>> sheetData);

    protected string ResolveKey(MemberInfo member)
    {
        return !string.IsNullOrWhiteSpace(Key) ? Key : member.Name;
    }

    protected static Type GetMemberType(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo prop => prop.PropertyType,
            _ => null
        };
    }

    protected static object GetMemberValue(object target, MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.GetValue(target),
            PropertyInfo prop => prop.GetValue(target),
            _ => null
        };
    }

    protected static void SetMemberValue(object target, MemberInfo member, object value)
    {
        if (member is FieldInfo field)
        {
            field.SetValue(target, value);
        }
        else if (member is PropertyInfo prop && prop.CanWrite)
        {
            prop.SetValue(target, value);
        }
    }
}