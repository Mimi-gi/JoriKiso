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

    // ドラッグ中のノード（複製されたもの）を保持
    private Node draggedNode = null;

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag - Creating duplicate node");
        
        // InferenceSuggestionPanel の content 内にあるかチェック
        if (InferenceSuggestionPanel.Instance != null)
        {
            var nodeRect = GetComponent<RectTransform>();
            if (InferenceSuggestionPanel.Instance.IsNodeInContent(nodeRect))
            {
                Debug.Log("Node is in InferenceSuggestionPanel content - creating duplicate");
                
                // 複製を作成し、マウス位置に配置
                draggedNode = DuplicateNodeAtPosition(eventData);
                if (draggedNode != null)
                {
                    // Pointer に複製を登録
                    if (Pointer.Instance != null)
                    {
                        Pointer.Instance.Register(draggedNode);
                    }
                    Debug.Log("Duplicate node created and registered to Pointer");
                }
                return;
            }
        }
        
        // 通常のドラッグの場合（複製ではない）
        draggedNode = this;
        if (Pointer.Instance != null)
        {
            Pointer.Instance.Register(this);
        }
    }

    /// <summary>
    /// このノードの複製を作成し、Canvas にマウス位置で配置する。
    /// </summary>
    public virtual Node DuplicateNodeAtPosition(PointerEventData eventData)
    {
        Node duplicated = Instantiate(gameObject).GetComponent<Node>();
        if (duplicated == null) return null;

        // Canvas に配置
        var duplicatedRT = duplicated.GetComponent<RectTransform>();
        duplicatedRT.SetParent(CanvasRect.Main, false);
        duplicatedRT.anchorMin = new Vector2(0.5f, 0.5f);
        duplicatedRT.anchorMax = new Vector2(0.5f, 0.5f);
        
        // マウス位置をCanvas座標に変換
        var canvasRect = CanvasRect.Main;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            duplicatedRT.anchoredPosition = localPoint;
        }
        else
        {
            duplicatedRT.position = eventData.position;
        }

        return duplicated;
    }

    /// <summary>
    /// このノードの複製を作成し、Canvas に配置する。
    /// 特定のノード型（SequentNode など）には特別な初期化を行う。
    /// </summary>
    public virtual Node DuplicateNode()
    {
        Node duplicated = Instantiate(gameObject).GetComponent<Node>();
        if (duplicated == null) return null;

        // Canvas に配置
        var duplicatedRT = duplicated.GetComponent<RectTransform>();
        var originalRT = GetComponent<RectTransform>();
        
        duplicatedRT.SetParent(CanvasRect.Main, false);
        duplicatedRT.anchoredPosition = originalRT.anchoredPosition;

        return duplicated;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        // ドラッグ対象を取得
        Node targetNode = draggedNode != null ? draggedNode : this;
        
        // ポインター位置に RectTransform を追従させる
        var rt = targetNode.GetComponent<RectTransform>();
        if (rt == null)
        {
            // 非UIの場合はワールド座標で追従
            targetNode.transform.position = eventData.position;
            return;
        }

        // draggedNode（複製）の場合、Canvas座標で計算
        if (draggedNode != null && draggedNode == targetNode)
        {
            var canvasRect = CanvasRect.Main;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                rt.anchoredPosition = localPoint;
            }
            else
            {
                rt.position = eventData.position;
            }
            return;
        }

        // 通常のドラッグの場合
        var parentRT = rt.parent as RectTransform;
        Vector2 localPoint2;
        if (parentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, eventData.position, eventData.pressEventCamera, out localPoint2))
        {
            rt.anchoredPosition = localPoint2;
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
        
        // draggedNode がある場合は、元のノードはそのまま（クローンは Pointer に登録されたままに）
        if (draggedNode != null && draggedNode != this)
        {
            // 複製物のドラッグが終了。以降の処理は Pointer 経由で行われる
            draggedNode = null;
        }
        else if (Pointer.Instance != null)
        {
            Pointer.Instance.Unregister();
        }
    }

}
