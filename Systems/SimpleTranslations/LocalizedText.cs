using System;

[Serializable]
public class LocalizedText
{
    public string Key;
    public string Value => string.IsNullOrEmpty(Key) ? null : SimpleTranslations.Instance.GetValue(Key);
    
    public static implicit operator string(LocalizedText localizedText)
    {
        return localizedText != null ? localizedText.Value : null;
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }
}
