using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 端をドラッグして横方向に伸縮可能であることを表すマーカーインターフェース
public interface IExpandable { }

public class InferenceBar : MonoBehaviour, IExpandable, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] GameObject PortPrefab;
    [SerializeField] RulePatch rulePatch;
    [SerializeField] float minWidth = 50f;
    [SerializeField] float maxWidth = 1000f;
    [SerializeField] float portVerticalOffset = 30f;   // バーからポートまでの縦方向オフセット
    [SerializeField] float minPortSpan = 60f;          // ポート間の最小スパン

    RectTransform rectTransform;

    // ドラッグ用
    Vector2 dragStartMousePos;
    Vector2 dragStartAnchoredPos;
    
    public List<Port> PremisesPorts = new List<Port>();
    public Port ConclusionPort;
    public bool isStart=false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        RefreshPorts();
    }

    // クリックされたときに Pointer に「最後に触った InferenceBar」として登録し、
    // 推論サジェストパネルの内容もこのバーに基づいてリフレッシュする
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Pointer.Instance != null)
        {
            Pointer.Instance.RegisterInferenceBar(this);
        }

        if (InferenceSuggestionPanel.Instance != null)
        {
            InferenceSuggestionPanel.Instance.RefreshFor(this);
        }
    }

    // 現在のバー幅に応じてポートを自動生成・整列する
    void RefreshPorts()
    {
        if (rectTransform == null || PortPrefab == null) return;

        float width = rectTransform.rect.width;
        if (width <= 0f) return;

        // isStart が true の場合、上側のポート（前提）は不要
        int desiredPremiseCount = 0;
        if (!isStart)
        {
            // 幅に応じて1個または2個に制限
            desiredPremiseCount = width >= 60f ? 2 : 1;
        }

        // PremisesPorts 数を desiredPremiseCount に揃える
        while (PremisesPorts.Count > desiredPremiseCount)
        {
            var last = PremisesPorts[PremisesPorts.Count - 1];
            if (last != null)
            {
                Destroy(last.gameObject);
            }
            PremisesPorts.RemoveAt(PremisesPorts.Count - 1);
        }

        while (PremisesPorts.Count < desiredPremiseCount)
        {
            var go = Instantiate(PortPrefab, rectTransform);
            var port = go.GetComponent<Port>();
            PremisesPorts.Add(port);
        }

        // 結論ポートがまだなければ生成
        if (ConclusionPort == null)
        {
            var go = Instantiate(PortPrefab, rectTransform);
            ConclusionPort = go.GetComponent<Port>();
        }

        // 各ポートの位置をバー直上に整列
        RectTransform parent = rectTransform; // ポートは常に InferenceBar の子
        float centerX = 0f; // バー自身のローカル中心
        float barTopY = rectTransform.rect.height * 0.5f + portVerticalOffset;

        int totalPorts = desiredPremiseCount + 1; // premises + conclusion
        if (totalPorts <= 0) return;

        float span = Mathf.Max(minPortSpan, width / totalPorts);

        for (int i = 0; i < PremisesPorts.Count; i++)
        {
            var port = PremisesPorts[i];
            if (port == null) continue;
            RectTransform pr = port.transform as RectTransform;
            if (pr == null) continue;

            float offsetIndex = i - (desiredPremiseCount - 1) * 0.5f;
            float x = centerX + offsetIndex * span;
            // すでにランタイムインスタンスの親が InferenceBar になっている前提のため、
            // Prefab アセットに対して SetParent が呼ばれないようチェックする
            if (pr.parent != parent)
            {
                pr.SetParent(parent, false);
            }
            pr.anchoredPosition = new Vector2(x, barTopY);
        }

        // 結論ポートは中央に配置
        if (ConclusionPort != null)
        {
            RectTransform cr = ConclusionPort.transform as RectTransform;
            if (cr != null)
            {
                if (cr.parent != parent)
                {
                    cr.SetParent(parent, false);
                }
                cr.anchoredPosition = new Vector2(centerX, -barTopY);
            }
        }
    }

    // ==== ドラッグでバー全体を移動 ==== 
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;
        dragStartMousePos = eventData.position;
        dragStartAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        Vector2 delta = eventData.position - dragStartMousePos;
        // キャンバス座標に合わせるため、親 RectTransform でローカルに変換
        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent != null)
        {
            Vector2 localStart;
            Vector2 localCurrent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, dragStartMousePos, eventData.pressEventCamera, out localStart);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out localCurrent);
            Vector2 localDelta = localCurrent - localStart;
            rectTransform.anchoredPosition = dragStartAnchoredPos + localDelta;
        }
        else
        {
            rectTransform.anchoredPosition = dragStartAnchoredPos + delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 今のところ特別な確定処理は不要
    }

    // 左端を、キャンバス座標 dragCanvasX の位置に合わせるように幅を更新する
    public void ResizeFromLeft(float dragCanvasX)
    {
        if (rectTransform == null) return;

        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null) return;

        // 現在の中心ローカル座標と幅から左端のキャンバスXを求める
        float halfWidth = rectTransform.rect.width * 0.5f;
        float centerCanvasX = rectTransform.anchoredPosition.x;
        float leftCanvasX = centerCanvasX - halfWidth;

        // 左端をドラッグ位置に移動したときの新しい幅
        float rawNewWidth = (leftCanvasX + rectTransform.rect.width) - dragCanvasX;
        float clampedWidth = Mathf.Clamp(rawNewWidth, minWidth, maxWidth);

        // 左端は dragCanvasX に固定しつつ、新しい幅から中心を計算
        float newHalfWidth = clampedWidth * 0.5f;
        float newCenterCanvasX = dragCanvasX + newHalfWidth;

        rectTransform.sizeDelta = new Vector2(clampedWidth, rectTransform.sizeDelta.y);
        rectTransform.anchoredPosition = new Vector2(newCenterCanvasX, rectTransform.anchoredPosition.y);

        RefreshPorts();
    }

    // 右端を、キャンバス座標 dragCanvasX の位置に合わせるように幅を更新する
    public void ResizeFromRight(float dragCanvasX)
    {
        if (rectTransform == null) return;

        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null) return;

        // 現在の中心ローカル座標と幅から右端のキャンバスXを求める
        float halfWidth = rectTransform.rect.width * 0.5f;
        float centerCanvasX = rectTransform.anchoredPosition.x;
        float rightCanvasX = centerCanvasX + halfWidth;

        // 右端をドラッグ位置に移動したときの新しい幅
        float rawNewWidth = dragCanvasX - (rightCanvasX - rectTransform.rect.width);
        float clampedWidth = Mathf.Clamp(rawNewWidth, minWidth, maxWidth);

        // 右端は dragCanvasX に固定しつつ、新しい幅から中心を計算
        float newHalfWidth = clampedWidth * 0.5f;
        float newCenterCanvasX = dragCanvasX - newHalfWidth;

        rectTransform.sizeDelta = new Vector2(clampedWidth, rectTransform.sizeDelta.y);
        rectTransform.anchoredPosition = new Vector2(newCenterCanvasX, rectTransform.anchoredPosition.y);

        RefreshPorts();
    }
}

// InferenceBar の左右端につけるドラッグハンドル

