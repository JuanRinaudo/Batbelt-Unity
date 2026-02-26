using UnityEngine;
using TMPro;

public class LocalizeTMPText : MonoBehaviour
{

    public TextMeshProUGUI textToLocalize;
    public string textKey = "";
    public TextModifier modifier;

#if UNITY_EDITOR
    private void Awake()
    {
        if(textToLocalize == null)
        {
            BatCore.LogWarning($"LocalizeTMPText: {gameObject.name} has no text asigned");
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
            textToLocalize = GetComponent<TextMeshProUGUI>();
        }

        if (textToLocalize != null && textKey != "")
        {
            textToLocalize.text = SimpleTranslations.GetText(textKey).ApplyModifier(modifier);
        }
        else
        {
            BatCore.LogWarning($"SimpleTranslations - No text set on LocalizeTMPText {gameObject.name}");
        }
    }

}
