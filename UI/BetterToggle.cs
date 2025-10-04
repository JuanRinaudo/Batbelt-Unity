using TMPro;
using AuraTween;
using UnityEngine;
using UnityEngine.UI;

public class BetterToggle : Toggle
{
    public TextMeshProUGUI Label;
    public Image Icon;

    public Tween _labelTween;
    public Tween _iconTween;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        switch (state)
        {
            case SelectionState.Normal:
                TransitionGraphicsColors(instant, colors.normalColor);
                break;
            case SelectionState.Highlighted:
                TransitionGraphicsColors(instant, colors.highlightedColor);
                break;
            case SelectionState.Pressed:
                TransitionGraphicsColors(instant, colors.pressedColor);
                break;
            case SelectionState.Selected:
                TransitionGraphicsColors(instant, colors.selectedColor);
                break;
            case SelectionState.Disabled:
                TransitionGraphicsColors(instant, colors.disabledColor);
                break;
        }
    }

    void TransitionGraphicsColors(bool instant, Color targetColor)
    {
        if (_labelTween.IsAlive)
            _labelTween.Cancel();
        if(_iconTween.IsAlive)
            _iconTween.Cancel();
        
        if (instant)
        {
            if (Label != null)
                Label.color = targetColor;
            if (Icon != null)
                Icon.color = targetColor;
        }
        else
        {
            if (Label != null)
                _labelTween = Label.TwColor(targetColor, colors.fadeDuration, Easer.Linear);
            if (Icon != null)
                _iconTween = Icon.TwColor(targetColor, colors.fadeDuration, Easer.Linear);
        }
    }
}
