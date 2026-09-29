using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class GridLayoutContentTransformScaler : MonoBehaviour
{
    public RectTransform TargetContainer;
    public bool UniformScale = true;

    RectTransform _rectTransform;
    GridLayoutGroup _gridLayoutGroup;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        AdjustScale();
    }

    void OnRectTransformDimensionsChange()
    {
        AdjustScale();
    }

    void Initialize()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_gridLayoutGroup == null)
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();

        if (TargetContainer == null && _rectTransform.parent != null)
            TargetContainer = _rectTransform.parent.GetComponent<RectTransform>();
    }

    public void AdjustScale()
    {
        Initialize();

        if (TargetContainer == null || _gridLayoutGroup == null)
            return;

        float refWidth = CalculateReferenceWidth();
        float targetWidth = TargetContainer.rect.width;

        if (refWidth <= 0f || targetWidth <= 0f)
            return;

        float scaleFactor = targetWidth / refWidth;

        if (UniformScale)
        {
            _rectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
        else
        {
            _rectTransform.localScale = new Vector3(scaleFactor, 1f, 1f);
        }
    }

    float CalculateReferenceWidth()
    {
        int columns = 1;

        if (_gridLayoutGroup.constraint == 
            GridLayoutGroup.Constraint.FixedColumnCount)
        {
            columns = Mathf.Max(1, _gridLayoutGroup.constraintCount);
        }
        else
        {
            // If not fixed column count, calculate based on active children
            int activeChildren = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    activeChildren++;
            }
            columns = Mathf.Max(1, activeChildren);
        }

        float totalPadding = _gridLayoutGroup.padding.left + 
                             _gridLayoutGroup.padding.right;
        float totalCellWidth = columns * _gridLayoutGroup.cellSize.x;
        float totalSpacing = (columns - 1) * _gridLayoutGroup.spacing.x;

        return totalPadding + totalCellWidth + totalSpacing;
    }
}