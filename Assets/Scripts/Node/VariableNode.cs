using UnityEngine;
using UnityEngine.EventSystems;
using FormalSystem.LK;
public class VariableNode : Node
{
    [SerializeField] Alphabet alphabet;
    [SerializeField] float len = 32;
    void Start()
    {
        length.Value = len;
        Formula = new Variable(alphabet);
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData); // フレームから離脱時の filled 解除
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