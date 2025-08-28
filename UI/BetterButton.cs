using TMPro;
using AuraTween;
using UnityEngine;
using UnityEngine.UI;

public class BetterButton : Button
{
    public TextMeshProUGUI Label;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (Label != null)
        {
            if (state == SelectionState.Normal)
            {
                if (instant)
                    Label.color = colors.normalColor;
                else
                    Label.TwColor(colors.normalColor, colors.fadeDuration, Easer.Linear);
            }
            else if (state == SelectionState.Highlighted)
            {
                if (instant)
                    Label.color = colors.highlightedColor;
                else
                    Label.TwColor(colors.highlightedColor, colors.fadeDuration, Easer.Linear);
            }
            else if (state == SelectionState.Pressed)
            {
                if (instant)
                    Label.color = colors.pressedColor;
                else
                    Label.TwColor(colors.pressedColor, colors.fadeDuration, Easer.Linear);
            }
            else if (state == SelectionState.Disabled)
            {
                if (instant)
                    Label.color = colors.disabledColor;
                else
                    Label.TwColor(colors.disabledColor, colors.fadeDuration, Easer.Linear);
            }
        }
    }
}
