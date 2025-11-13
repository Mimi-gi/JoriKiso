using UnityEngine;
using UnityEngine.EventSystems;
using FormalSystem.LK;
using R3;
using Unity.VisualScripting;
public class BinaryNode : Node
{
    [SerializeField] float len = 18;
    [SerializeField] Frame leftframe;
    [SerializeField] Frame rightframe;
    [SerializeField] Kind kind;
    public Frame LeftFrame => leftframe;
    public Frame RightFrame => rightframe;
    void Update()
    {
        var l = (LeftFrame != null && LeftFrame.Node != null) ? LeftFrame.Node.Length.CurrentValue : 32f;
        var r = (RightFrame != null && RightFrame.Node != null) ? RightFrame.Node.Length.CurrentValue : 32f;
        length.Value = len + l + r;
    }
    void Start(){

    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData); // フレーム解除処理
        this.transform.SetAsLastSibling();
        Pointer.Instance.Register(this);
        Pointer.Instance.Node.GetComponent<RectTransform>().SetParent(CanvasRect.Main);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        Pointer.Instance.Unregister();
    }
}
