using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ルールノードを受け取るパッチ。<br/>
/// ・ルールノードがドロップされたら、このオブジェクトの子にして anchoredPosition=(0,0) に配置する。<br/>
/// ・そのルールノードが再びドラッグ開始されたら、子から外す。<br/>
/// </summary>
public class RulePatch : MonoBehaviour, IDropHandler
{
    [SerializeField] RectTransform rectTransform;

    // 現在このパッチに載っているルールノード
    public RuleNode CurrentRuleNode { get; private set; }

    void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// ルールノードがここにドロップされたときに呼ばれる。
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("[RulePatch] OnDrop called.");
        // Pointer ベース: 現在ドラッグ中の Node から RuleNode を取得
        if (Pointer.Instance == null || Pointer.Instance.Node == null)
        {
            Debug.Log("[RulePatch] Pointer has no active node.");
            return;
        }

        var ruleNode = Pointer.Instance.Node as RuleNode;
        if (ruleNode == null)
        {
            Debug.Log("[RulePatch] Dropped node is not RuleNode.");
            return;
        }

        // すでに何か載っている場合は上書き（必要なら拒否に変更も可）
        if (CurrentRuleNode != null && CurrentRuleNode != ruleNode)
        {
            // 以前の子を外しておく
            var prevRect = CurrentRuleNode.GetComponent<RectTransform>();
            if (prevRect != null && prevRect.parent == rectTransform)
            {
                prevRect.SetParent(null);
            }
        }

        CurrentRuleNode = ruleNode;

        var nodeRect = ruleNode.GetComponent<RectTransform>();
        if (nodeRect != null)
        {
            nodeRect.SetParent(rectTransform);
            nodeRect.anchoredPosition = Vector2.zero;
        }

        // ルールがこのパッチ上で確定したので、推論サジェストパネルを更新
        if (InferenceSuggestionPanel.Instance != null && Pointer.Instance != null)
        {
            var bar = Pointer.Instance.LastInferenceBar;
            if (bar != null)
            {
                InferenceSuggestionPanel.Instance.RefreshFor(bar);
            }
        }
    }

    /// <summary>
    /// ルールノードのドラッグ開始時に、このパッチから外すためのメソッド。<br/>
    /// RuleNode 側から OnBeginDrag などで呼んでもらう想定。
    /// </summary>
    public void DetachRuleNode(RuleNode node)
    {
        if (node == null) return;
        if (CurrentRuleNode != node) return;

        var rect = node.GetComponent<RectTransform>();
        if (rect != null && rect.parent == rectTransform)
        {
            rect.SetParent(CanvasRect.Main);
        }

        CurrentRuleNode = null;
    }
}
