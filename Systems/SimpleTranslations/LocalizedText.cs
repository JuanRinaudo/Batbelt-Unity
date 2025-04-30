using System;

[Serializable]
public class LocalizedText
{
    public string Key;
    public string Value => SimpleTranslations.Instance.GetValue(Key);
}