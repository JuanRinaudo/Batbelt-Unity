#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

public class ListViewSelector : MouseManipulator
{
    public ListViewSelector()
    {
        activators.Add(new ManipulatorActivationFilter
        {
            button = MouseButton.LeftMouse
        });
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Command
            });
        }
        else
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Control
            });
        }
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void OnMouseDown(MouseDownEvent e)
    {
        var element = e.target as VisualElement;
        if (element == null) return;

        if (IsChildOf<ListView>(element))
        {
            e.StopImmediatePropagation();
        }
    }

    private bool IsChildOf<T>(VisualElement element) where T : VisualElement
    {
        VisualElement currentParent = element.parent;
        while (currentParent != null)
        {
            if (currentParent is T) return true;

            currentParent = currentParent.parent;
        }

        return false;
    }
}
#endif