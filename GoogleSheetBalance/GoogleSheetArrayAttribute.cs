using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GoogleSheetArrayAttribute : Attribute
{
    public string Key { get; }

    public GoogleSheetArrayAttribute(string key = null)
    {
        Key = key;
    }
}