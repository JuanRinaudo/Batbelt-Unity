using TMPro;
using SimpleTweens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BetterToggle : Toggle
{
    public TextMeshProUGUI Label;
    public Image Icon;
    
    public bool LabelSyncFullColor = false;
    public bool IconSyncFullColor = false;

    public Image Overlay;
    public Color OverlayOnColor = new Color(0, 0, 0, 0);
    public Color OverlayOffColor = new Color(0, 0, 0, 0.5f);

    public Tween _labelTween;
    public Tween _iconTween;
    public Tween _valueOverlayTween;

    protected override void Start()
    {
        base.Start();
        
        onValueChanged.AddListener(OnValueChanged);
    }

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
        _labelTween.TryCancel();
        _iconTween.TryCancel();
        _valueOverlayTween.TryCancel();
        
        if (instant)
        {
            if (Label != null)
                Label.color = LabelSyncFullColor ? targetColor : Label.color.WithA(targetColor.a);
            if (Icon != null)
                Icon.color = IconSyncFullColor ? targetColor : Icon.color.WithA(targetColor.a);
            if (Overlay != null)
                Overlay.color = isOn ? OverlayOnColor : OverlayOffColor;
        }
        else
        {
            if (Label != null)
                _labelTween = LabelSyncFullColor ? Label.TwColor(targetColor, colors.fadeDuration, Easer.Linear) : Label.TwTextAlpha(targetColor.a, colors.fadeDuration, Easer.Linear);
            if (Icon != null)
                _iconTween = IconSyncFullColor ? Icon.TwColor(targetColor, colors.fadeDuration, Easer.Linear) : Icon.TwAlpha(targetColor.a, colors.fadeDuration, Easer.Linear);
            if (Overlay != null)
                _valueOverlayTween = Overlay.TwColor(isOn ? OverlayOnColor : OverlayOffColor, colors.fadeDuration, Easer.Linear);
        }
    }

    public void OnValueChanged(bool isOn)
    {
        _valueOverlayTween.TryCancel();
        
        if (Overlay != null)
            _valueOverlayTween = Overlay.TwColor(isOn ? OverlayOnColor : OverlayOffColor, colors.fadeDuration, Easer.Linear);
    }

    protected override void OnDestroy()
    {
        onValueChanged.RemoveListener(OnValueChanged);
    }
}
