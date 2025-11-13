using UnityEngine;
using UnityEngine.EventSystems;
using FormalSystem.LK;
using R3;
public class BinaryNode : Node
{
    [SerializeField] float len = 18;
    [SerializeField] Frame leftframe;
    [SerializeField] Frame rightframe;
    [SerializeField] Kind kind;
    public Frame LeftFrame => leftframe;
    public Frame RightFrame => rightframe;
    ReactiveProperty<bool> isValid = new ReactiveProperty<bool>(false);
    void Update()
    {
        var l = (LeftFrame != null && LeftFrame.Node != null) ? LeftFrame.Node.Length.CurrentValue : 32f;
        var r = (RightFrame != null && RightFrame.Node != null) ? RightFrame.Node.Length.CurrentValue : 32f;
        length.Value = len + l + r;
        isValid.Value = (LeftFrame != null && LeftFrame.Node != null && LeftFrame.Node.Formula != null
                        && RightFrame != null && RightFrame.Node != null && RightFrame.Node.Formula != null);
    }
    void Start(){
        isValid.Subscribe(valid => {
            if(valid){
                switch(kind){
                    case Kind.And:
                        Formula = new And(LeftFrame.Node.Formula, RightFrame.Node.Formula);
                        break;
                    case Kind.Or:
                        Formula = new Or(LeftFrame.Node.Formula, RightFrame.Node.Formula);
                        break;
                    case Kind.Implication:
                        Formula = new Implication(LeftFrame.Node.Formula, RightFrame.Node.Formula);
                        break;
                }
            } else {
                Formula = null;
            }
        });
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
