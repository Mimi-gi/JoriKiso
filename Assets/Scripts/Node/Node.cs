using FormalSystem.LK;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
public class Node : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    protected ReactiveProperty<float> length { get; private set; } = new ReactiveProperty<float>(0);
    public  ReadOnlyReactiveProperty<float> Length => length;
    public Formula Formula { get; set; }
    // このノードが現在格納されている Frame。ドロップ時に設定され、ドラッグ開始時に解除される。

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        // ポインター位置に RectTransform を追従させる
        var rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            // 非UIの場合はワールド座標で追従
            transform.position = eventData.position;
            return;
        }

        var parentRT = rt.parent as RectTransform;
        Vector2 localPoint;
        if (parentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            rt.anchoredPosition = localPoint;
        }
        else
        {
            // 親が無い/変換できない場合はスクリーン座標に直接配置
            rt.position = eventData.position;
        }
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
    }

}
