using UnityEngine;

public class SafeAreaContainer : MonoBehaviour
{
    public RectTransform Target;

    void Awake()
    {
        var safeArea = Screen.safeArea;
        var minAnchor = safeArea.position;
        var maxAnchor = minAnchor + safeArea.size;
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;
        Target.anchorMin = minAnchor;
        Target.anchorMax = maxAnchor;
    }
}
