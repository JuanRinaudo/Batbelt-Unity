using System;

[Serializable]
public class LocalizedText
{
    public string Key;
    public string Value => SimpleTranslations.Instance.GetValue(Key);
    
    public static implicit operator string(LocalizedText localizedText)
    {
        return localizedText != null ? localizedText.Value : null;
    }
}
