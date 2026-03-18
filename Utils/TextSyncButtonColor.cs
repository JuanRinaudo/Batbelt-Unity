using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextSyncButtonColor : MonoBehaviour
{
    public Button Target;
    public TextMeshProUGUI Text;

    void Update()
    {
        var newColor = Target.interactable ? Target.colors.normalColor : Target.colors.disabledColor;
        if(Text.color != newColor)
            Text.color = newColor;
    }
}
