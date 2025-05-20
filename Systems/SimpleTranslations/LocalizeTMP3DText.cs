using System;
using UnityEngine;
using TMPro;

public class LocalizeTMP3DText : MonoBehaviour
{

    public TextMeshPro textToLocalize;
    public string textKey = "";
    public TextModifier modifier;

#if UNITY_EDITOR
    private void Awake()
    {
        if(textToLocalize == null)
        {
            BatCore.LogWarning($"LocalizeTMPText: {gameObject.name} has no text assigned");
        }
    }

    private void OnValidate()
    {
        UpdateLocalization();
    }
#endif

    public void Start()
    {
        UpdateLocalization();

        SimpleTranslations.LanguageChanged += UpdateLocalization;
    }

    public void UpdateLocalization()
    {
        if(textToLocalize == null)
        {
            textToLocalize = GetComponent<TextMeshPro>();
        }

        if (textToLocalize != null && textKey != "")
        {
            textToLocalize.text = SimpleTranslations.GetText(textKey).ApplyModifier(modifier);
        }
        else
        {
            BatCore.LogError($"SimpleTranslations - No text set on LocalizeTMP3DText {gameObject.name}");
        }
    }

}
