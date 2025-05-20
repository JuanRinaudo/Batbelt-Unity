using UnityEngine;
using TMPro;

public class LocalizeTMPGroupText : MonoBehaviour
{

    public TextMeshProUGUI[] textGroup;
    public string[] textGroupKeys = new string[0];
    public TextModifier[] textGroupModifiers = new TextModifier[0];
    
#if UNITY_EDITOR
    private void Awake()
    {
        for (int textIndex = 0; textIndex < textGroup.Length; ++textIndex)
        {
            TextMeshProUGUI textToLocalize = textGroup[textIndex];
            if(textToLocalize == null)
            {
                Debug.LogWarning($"LocalizeTMPGroupText: {gameObject.name}, index {textIndex} has no text asigned");
            }
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
        for (int textIndex = 0; textIndex < textGroup.Length; ++textIndex)
        {
            TextMeshProUGUI textToLocalize = textGroup[textIndex];
            string textKey = textGroupKeys[textIndex];
            TextModifier modifier = textGroupModifiers[textIndex];

            if (textToLocalize == null)
            {
                textToLocalize = GetComponent<TextMeshProUGUI>();
            }

            if (textToLocalize != null && textKey != "")
            {
                textToLocalize.text = SimpleTranslations.GetText(textKey).ApplyModifier(modifier);
            }
            else
            {
                BatCore.LogError($"SimpleTranslations - No text set on LocalizeTMPGroupText {gameObject.name}");
            }
        }
    }

}
