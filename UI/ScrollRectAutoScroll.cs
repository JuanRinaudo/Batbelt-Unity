using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float ScrollSpeed = 10f;
    
    bool _mouseOver;

    List<Selectable> _selectables = new();
    ScrollRect _scrollRect;

    Vector2 _nextScrollPosition = Vector2.up;
    
    void OnEnable()
    {
        if (_scrollRect)
            _scrollRect.content.GetComponentsInChildren(_selectables);
        
        ScrollToSelected(true);
    }
    
    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }
    
    void Start()
    {
        if (_scrollRect)
            _scrollRect.content.GetComponentsInChildren(_selectables);
        
        ScrollToSelected(true);
    }
    
    void Update()
    {
        ScrollToSelected(false);
        
        if (!_mouseOver)
            _scrollRect.normalizedPosition = Vector2.Lerp(_scrollRect.normalizedPosition, _nextScrollPosition, ScrollSpeed * Time.deltaTime);
        else
            _nextScrollPosition = _scrollRect.normalizedPosition;
    }
    
    void ScrollToSelected(bool quickScroll)
    {
        int selectedIndex = -1;
        Selectable selectedElement = EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

        if (selectedElement) {
            selectedIndex = _selectables.IndexOf(selectedElement);
        }
        if (selectedIndex > -1) {
            if (quickScroll) {
                _scrollRect.normalizedPosition = new Vector2(0, 1 - (selectedIndex / ((float)_selectables.Count - 1)));
                _nextScrollPosition = _scrollRect.normalizedPosition;
            }
            else {
                _nextScrollPosition = new Vector2(0, 1 - (selectedIndex / ((float)_selectables.Count - 1)));
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseOver = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseOver = false;
    }
}