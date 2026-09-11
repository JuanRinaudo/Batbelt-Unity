using System.Collections.Generic;
using System.Reflection;

public class GoogleSheetValueAttribute : GoogleSheetBindingAttribute
{
    public GoogleSheetValueAttribute(string key = null) : base(key) { }

    public override bool Apply(
        object target,
        MemberInfo member,
        IReadOnlyDictionary<string, List<string>> sheetData
    )
    {
        string lookupKey = ResolveKey(member);
        if (!sheetData.TryGetValue(lookupKey, out var values) ||
            values.Count == 0)
        {
            return false;
        }

        object parsedValue = GoogleSheetConverter.ConvertValue(
            values[0],
            GetMemberType(member)
        );

        SetMemberValue(target, member, parsedValue);
        return true;
    }
}