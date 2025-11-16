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

    private float lastNodeLength = -1f;

    void Update()
    {
        // 子ノードがある場合、そのLengthを監視して フレーム幅を更新
        if (Node != null)
        {
            float currentNodeLength = Node.Length.CurrentValue;
            // ノードの Length が変わったら、フレーム幅を更新
            if (!Mathf.Approximately(currentNodeLength, lastNodeLength))
            {
                lastNodeLength = currentNodeLength;
                rectTransform.sizeDelta = new Vector2(currentNodeLength + 2, 32);
                Length = currentNodeLength + 2;
                Debug.Log($"[Frame.Update] Updated frame width to {rectTransform.sizeDelta.x} (node length: {currentNodeLength})");
            }
        }
        
        // ノードが外された場合の検出
        if (Node != null && Node.GetComponent<RectTransform>().parent != rectTransform)
        {
            Debug.Log("[Frame.Update] Node was removed from this frame");
            Node = null;
            filled = false;
            rectTransform.sizeDelta = new Vector2(32f, 32);
            Length = 32f;
            lastNodeLength = -1f;
        }
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (Node != null)
        {
            Debug.Log("Frame is already filled - cannot drop new node");
            return;
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

    // 静的生成用: ポインタイベントなしで直接 Node を配置するAPI
    public void SetNodeDirect(Node node)
    {
        if (node == null) return;
        Node = node;
        var rect = Node.GetComponent<RectTransform>();
        if (rect != null)
        {
            // ノードの Length を取得（ReactiveProperty なので .CurrentValue）
            float nodeLength = node.Length.CurrentValue;
            Debug.Log($"[Frame.SetNodeDirect] Node length: {nodeLength}");
            
            // フレームサイズ更新
            rectTransform.sizeDelta = new Vector2(nodeLength + 2, 32);
            Length = nodeLength + 2;
            Debug.Log($"[Frame.SetNodeDirect] Frame sizeDelta set to: {rectTransform.sizeDelta}");
            
            rect.SetParent(rectTransform);
            
            switch (direction)
            {
                case Direction.Left:
                    if (Node.TryGetComponent<VariableNode>(out var varNodeL))
                        rect.anchoredPosition = new Vector2(1, 0);
                    else if (Node.TryGetComponent<UnaryNode>(out var unaryNodeL))
                        rect.anchoredPosition = new Vector2(-nodeLength / 2 + 6, 0);
                    else if (Node.TryGetComponent<BinaryNode>(out var binaryNodeL))
                    {
                        var childL = (binaryNodeL.LeftFrame != null && binaryNodeL.LeftFrame.Node != null)
                                     ? binaryNodeL.LeftFrame.Node.Length.CurrentValue
                                     : (binaryNodeL.LeftFrame != null ? binaryNodeL.LeftFrame.Length : 32f);
                        var childR = (binaryNodeL.RightFrame != null && binaryNodeL.RightFrame.Node != null)
                                     ? binaryNodeL.RightFrame.Node.Length.CurrentValue
                                     : (binaryNodeL.RightFrame != null ? binaryNodeL.RightFrame.Length : 32f);
                        rect.anchoredPosition = new Vector2((childR - childL) / 2 + 1, 0);
                    }
                    break;
                case Direction.Right:
                    if (Node.TryGetComponent<VariableNode>(out var varNode))
                        rect.anchoredPosition = new Vector2(-1, 0);
                    else if (Node.TryGetComponent<UnaryNode>(out var unaryNode))
                        rect.anchoredPosition = new Vector2(-nodeLength / 2 + 4, 0);
                    else if (Node.TryGetComponent<BinaryNode>(out var binaryNode))
                    {
                        var childL = (binaryNode.LeftFrame != null && binaryNode.LeftFrame.Node != null)
                                     ? binaryNode.LeftFrame.Node.Length.CurrentValue
                                     : (binaryNode.LeftFrame != null ? binaryNode.LeftFrame.Length : 32f);
                        var childR = (binaryNode.RightFrame != null && binaryNode.RightFrame.Node != null)
                                     ? binaryNode.RightFrame.Node.Length.CurrentValue
                                     : (binaryNode.RightFrame != null ? binaryNode.RightFrame.Length : 32f);
                        rect.anchoredPosition = new Vector2((childL - childR) / 2 - 1, 0);
                    }
                    break;
            }
        }
        filled = true;
        
        // 初期値を設定して Update で監視開始
        lastNodeLength = node.Length.CurrentValue;
        Debug.Log("[Frame.SetNodeDirect] Node set, Update will monitor length");
    }
}

public enum Direction
{
    Left,
    Right
}