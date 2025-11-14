using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using R3;
using FormalSystem.LK;

public class Frame : MonoBehaviour, IDropHandler
{
    [SerializeField] Image frameImage;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Direction direction;
    [SerializeField] Node parent;
    bool filled = false;

    public Node Node { get; private set; }
    public float Length { get; private set; } = 32f;
    public RectTransform RectTransform { get { return rectTransform; } }


    public void OnDrop(PointerEventData eventData)
    {
        if (Node != null)
        {
            Debug.Log("Frame is already filled");
            //return;
        }
        if (Pointer.Instance.Node != null)
        {
            Debug.Log("Node dropped into frame");
            Node = Pointer.Instance.Node;
            var rect = Node.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(Node.Length.CurrentValue + 2, 32);
            Length = Node.Length.CurrentValue + 2;
            rect.SetParent(rectTransform);
            switch (direction)
            {
                case Direction.Left:
                    if (Node.TryGetComponent<VariableNode>(out var varNodeL))
                    {
                        Debug.Log("命題変項左セット");
                        rect.anchoredPosition = new Vector2(1, 0);
                    }
                    else if (Node.TryGetComponent<UnaryNode>(out var unaryNodeL))
                    {
                        Debug.Log("一項ノード左セット");
                        rect.anchoredPosition = new Vector2(-Node.Length.CurrentValue / 2 + 6, 0);
                    }
                    else if (Node.TryGetComponent<BinaryNode>(out var binaryNodeL))
                    {
                        Debug.Log("二項ノード左セット");
                        var childL = (binaryNodeL.LeftFrame != null && binaryNodeL.LeftFrame.Node != null)
                                     ? binaryNodeL.LeftFrame.Node.Length.CurrentValue
                                     : binaryNodeL.LeftFrame.Length;
                        var childR = (binaryNodeL.RightFrame != null && binaryNodeL.RightFrame.Node != null)
                                     ? binaryNodeL.RightFrame.Node.Length.CurrentValue
                                     : binaryNodeL.RightFrame.Length;
                        rect.anchoredPosition = new Vector2((childR - childL) / 2 + 1, 0);
                    }
                    break;
                case Direction.Right:
                    if (Node.TryGetComponent<VariableNode>(out var varNode))
                    {
                        Debug.Log("命題変項右セット");
                        rect.anchoredPosition = new Vector2(-1, 0);
                    }
                    else if (Node.TryGetComponent<UnaryNode>(out var unaryNode))
                    {
                        Debug.Log("一項ノード右セット");
                        rect.anchoredPosition = new Vector2(-Node.Length.CurrentValue / 2 + 4, 0);
                    }
                    else if (Node.TryGetComponent<BinaryNode>(out var binaryNode))
                    {
                        Debug.Log("二項ノード右セット");
                        var childL = (binaryNode.LeftFrame != null && binaryNode.LeftFrame.Node != null)
                                     ? binaryNode.LeftFrame.Node.Length.CurrentValue
                                     : binaryNode.LeftFrame.Length;
                        var childR = (binaryNode.RightFrame != null && binaryNode.RightFrame.Node != null)
                                     ? binaryNode.RightFrame.Node.Length.CurrentValue
                                     : binaryNode.RightFrame.Length;
                        rect.anchoredPosition = new Vector2((childL - childR) / 2 - 1, 0);
                    }
                    break;
            }
            filled = true;
            Pointer.Instance.Unregister();
        }
        else
        {
            Debug.LogError("No node registered in Pointer");
        }
    }

    public void UnFilled()
    {
        filled = false;
    }
}

public enum Direction
{
    Left,
    Right
}