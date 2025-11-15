using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InferenceBarEdgeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Side { Left, Right }

    [SerializeField] Side side;
    [SerializeField] InferenceBar targetBar;

    RectTransform handleRect;

    void Awake()
    {
        handleRect = GetComponent<RectTransform>();
        if (targetBar == null)
        {
            targetBar = GetComponentInParent<InferenceBar>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 特に何もしないが、将来のために残しておく
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetBar == null) return;

        // スクリーン座標からキャンバス基準のローカル座標に変換
        RectTransform canvasRect = targetBar.transform.parent as RectTransform;
        if (canvasRect == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        if (side == Side.Left)
        {
            // 左端のキャンバスXをカーソルのXに合わせる
            targetBar.ResizeFromLeft(localPoint.x);
        }
        else
        {
            // 右端のキャンバスXをカーソルのXに合わせる
            targetBar.ResizeFromRight(localPoint.x);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 将来、確定時にポートの自動再配置などを呼びたい場合はここで呼ぶ
        UpdateHandlePositionToEdge();
    }

    void LateUpdate()
    {
        // バーのサイズ変更中もハンドルが端に居続けるようにする
        UpdateHandlePositionToEdge();
    }

    private void UpdateHandlePositionToEdge()
    {
        if (handleRect == null || targetBar == null) return;

        RectTransform barRect = targetBar.transform as RectTransform;
        if (barRect == null) return;

        // 親は InferenceBar の RectTransform を想定
        // 左ハンドル: 左端に、右ハンドル: 右端に anchoredPosition を合わせる
        float halfWidth = barRect.rect.width * 0.5f;
        Vector2 pos = handleRect.anchoredPosition;

        pos.y = 0f; // 垂直方向は中央にそろえる
        pos.x = (side == Side.Left) ? -halfWidth : halfWidth;

        handleRect.anchoredPosition = pos;
    }
}
