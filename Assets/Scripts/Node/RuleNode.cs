using UnityEngine;
using FormalSystem.LK;
using UnityEngine.EventSystems;

// Node を継承して Pointer ベースのドラッグと統一
public class RuleNode : Node
{
    [SerializeField] Rule rule;
    public Rule Rule => rule;
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        Debug.Log("RuleNode Begin Drag");

        // もし親に RulePatch があれば、「パッチから外す」
        var parentPatch = GetComponentInParent<RulePatch>();
        if (parentPatch != null)
        {
            parentPatch.DetachRuleNode(this);
        }

        // ドラッグ開始時に一番前面に移動（他ノードと同様の見た目に）
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.SetAsLastSibling();
        }

        // Pointer に自分を登録（他の Node と同じドラッグ管理に載せる）
        if (Pointer.Instance != null)
        {
            Pointer.Instance.Register(this);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // 移動処理は Node 基底クラスに任せる
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("RuleNode End Drag");


            Pointer.Instance.Unregister();
    }
}