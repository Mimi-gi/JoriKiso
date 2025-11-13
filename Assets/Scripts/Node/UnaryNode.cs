using UnityEngine;
using FormalSystem.LK;
using UnityEngine.EventSystems;
using R3;
public class UnaryNode :Node
{
    [SerializeField] float len = 12;
    [SerializeField] Frame frame;
    [SerializeField] Kind kind;
    public Frame Frame => frame;
    ReactiveProperty<bool> isValid = new ReactiveProperty<bool>(false);
    void Start(){
        isValid.Subscribe(valid => {
            if(valid){
                switch(kind){
                    case Kind.Not:
                        Formula = new Not(Frame.Node.Formula);
                        break;
                }
            } else {
                Formula = null;
            }
        });
    }
    void Update()
    {
        var c = (Frame != null && Frame.Node != null) ? Frame.Node.Length.CurrentValue : 32f;
        length.Value = len + c;
        isValid.Value = (Frame != null && Frame.Node != null && Frame.Node.Formula != null);
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