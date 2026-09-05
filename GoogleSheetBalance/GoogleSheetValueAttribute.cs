using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GoogleSheetValueAttribute : Attribute
{
    public string Key { get; }

    public GoogleSheetValueAttribute(string key = null)
    {
        Key = key;
    }
}