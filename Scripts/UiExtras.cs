using UnityEngine;
using UnityEngine.EventSystems;

public static class UiExtras
{
    public static bool IsInputHoveringUI()
    {
        return (Application.isMobilePlatform && EventSystem.current.IsPointerOverGameObject(0)) || EventSystem.current.IsPointerOverGameObject();
    }
}