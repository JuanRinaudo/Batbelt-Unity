using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextMatchButtonColor : MonoBehaviour
{
    public Button Target;
    public TextMeshProUGUI Text;

    void Update()
    {
        Text.color = Target.interactable ? Target.colors.normalColor : Target.colors.disabledColor;
    }
}
