using UnityEngine;

public class SafeAreaContainer : MonoBehaviour
{
    public RectTransform Target;

    Rect _lastSafeArea;
    Vector2Int _lastScreenSize;

    void OnEnable()
    {
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != _lastSafeArea || Screen.width != _lastScreenSize.x || Screen.height != _lastScreenSize.y)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        var width = Screen.width;
        var height = Screen.height;
        if (width <= 0 || height <= 0)
            return;

        var safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(width, height);

        if (width > height)
        {
            // Keep landscape content centered, even when only one side has a cutout.
            var horizontalInset = Mathf.Max(safeArea.xMin, width - safeArea.xMax);
            safeArea.xMin = horizontalInset;
            safeArea.xMax = width - horizontalInset;
        }

        var minAnchor = safeArea.position;
        var maxAnchor = minAnchor + safeArea.size;
        minAnchor.x /= width;
        minAnchor.y /= height;
        maxAnchor.x /= width;
        maxAnchor.y /= height;
        Target.anchorMin = minAnchor;
        Target.anchorMax = maxAnchor;
    }
}
