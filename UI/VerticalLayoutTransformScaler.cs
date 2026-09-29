using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class VerticalFitTransformScaler : MonoBehaviour
{
    [Tooltip("Container to fit into. Defaults to parent if empty.")]
    [SerializeField] private RectTransform targetContainer;

    [Tooltip("Uniformly scales X and Y so elements don't get squished.")]
    [SerializeField] private bool uniformScale = true;

    [Tooltip(
        "If true, only shrinks to fit when content is too big, " +
        "never scales up beyond 1.0."
    )]
    [SerializeField] private bool onlyScaleDown = false;

    private RectTransform _rectTransform;
    private VerticalLayoutGroup _verticalLayoutGroup;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        AdjustScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        AdjustScale();
    }

    private void OnTransformChildrenChanged()
    {
        AdjustScale();
    }

    private void Initialize()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_verticalLayoutGroup == null)
            _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();

        if (targetContainer == null && _rectTransform.parent != null)
            targetContainer = _rectTransform.parent.GetComponent<RectTransform>();
    }

    public void AdjustScale()
    {
        Initialize();

        if (targetContainer == null || _verticalLayoutGroup == null)
            return;

        float refHeight = CalculateReferenceHeight();
        float refWidth = CalculateReferenceWidth();

        float targetHeight = targetContainer.rect.height;
        float targetWidth = targetContainer.rect.width;

        if (refHeight <= 0f || targetHeight <= 0f)
            return;

        // Primary scale factor: Vertical fit
        float scaleY = targetHeight / refHeight;

        // Scale factor required if horizontal limits are exceeded
        float scaleX = refWidth > 0f ? (targetWidth / refWidth) : scaleY;

        // Prioritize vertical fit. Only scale down further if it overflows horizontally.
        float finalScale = scaleY;
        if (scaleX < scaleY)
        {
            finalScale = scaleX;
        }

        if (onlyScaleDown && finalScale > 1f)
        {
            finalScale = 1f;
        }

        if (uniformScale)
        {
            _rectTransform.localScale = new Vector3(
                finalScale,
                finalScale,
                1f
            );
        }
        else
        {
            float nonUniformY = onlyScaleDown && scaleY > 1f ? 1f : scaleY;
            float nonUniformX = onlyScaleDown && scaleX > 1f ? 1f : scaleX;

            _rectTransform.localScale = new Vector3(
                Mathf.Min(1f, nonUniformX),
                nonUniformY,
                1f
            );
        }
    }

    private float CalculateReferenceHeight()
    {
        float totalChildrenHeight = 0f;
        int activeChildCount = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;

            if (child.TryGetComponent<ILayoutIgnorer>(out var ignorer) &&
                ignorer.ignoreLayout)
                continue;

            RectTransform childRect = child as RectTransform;
            if (childRect == null)
                continue;

            float h = LayoutUtility.GetPreferredHeight(childRect);
            if (h <= 0f)
                h = childRect.rect.height;

            totalChildrenHeight += h;
            activeChildCount++;
        }

        if (activeChildCount == 0)
            return 0f;

        float totalSpacing = (activeChildCount - 1) * _verticalLayoutGroup.spacing;
        float totalPadding = _verticalLayoutGroup.padding.top +
                             _verticalLayoutGroup.padding.bottom;

        return totalPadding + totalChildrenHeight + totalSpacing;
    }

    private float CalculateReferenceWidth()
    {
        float maxChildWidth = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf)
                continue;

            if (child.TryGetComponent<ILayoutIgnorer>(out var ignorer) &&
                ignorer.ignoreLayout)
                continue;

            RectTransform childRect = child as RectTransform;
            if (childRect == null)
                continue;

            float w = LayoutUtility.GetPreferredWidth(childRect);
            if (w <= 0f)
                w = childRect.rect.width;

            if (w > maxChildWidth)
                maxChildWidth = w;
        }

        float totalPadding = _verticalLayoutGroup.padding.left +
                             _verticalLayoutGroup.padding.right;

        return totalPadding + maxChildWidth;
    }
}