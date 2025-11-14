using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform parentRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ここで必要なら初期位置記録など
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;
        Vector2 localPoint;
        if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 必要なら確定処理
    }
}
