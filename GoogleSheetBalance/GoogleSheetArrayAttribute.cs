using System.Collections.Generic;
using System.Reflection;

public class GoogleSheetArrayAttribute : GoogleSheetBindingAttribute
{
    public GoogleSheetArrayAttribute(string key = null) : base(key) { }

    public override bool Apply(object target, MemberInfo member, IReadOnlyDictionary<string, List<string>> sheetData)
    {
        string lookupKey = ResolveKey(member);
        if (!sheetData.TryGetValue(lookupKey, out var values))
        {
            return false;
        }

        object collection = GoogleSheetConverter.CreateCollection(
            GetMemberType(member),
            values
        );

        if (collection == null) return false;

        SetMemberValue(target, member, collection);
        return true;
    }
}