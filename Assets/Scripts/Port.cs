using UnityEngine;
using UnityEngine.EventSystems;

// UI 上のポート。クリックイベント(IPointerClickHandler)で接続マネージャへ通知する。
public class Port : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SequentNode parentNode;
    public SequentNode ParentNode => parentNode;

    // このポートが元々（初期状態で）親ノードを持っていたかどうか
    bool hadParentAtStart;
    // 現在何かと接続されているかどうか（接続ライン生成時に true、切断時に false を想定）
    public bool IsConnected { get; set; }

    void Start()
    {
        // シーン開始時点の親ノード有無を記録しておく
        hadParentAtStart = (parentNode != null);
    }

    void Update()
    {
        // 「もともと親ノードを持たないポート」で、かつ現在接続が無い場合は、
        // 親ノード参照をクリアして InferenceBar 側の状態をリセットする。
        if (!hadParentAtStart && !IsConnected && parentNode != null)
        {
            parentNode = null;
        }
    }

    // 接続確定時などに、接続先ノードを親として紐付けるためのAPI
    public void SetParentNode(SequentNode node)
    {
        parentNode = node;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (BezierConnectionManager.Instance == null) return;

        // 左クリック: 接続開始／確定
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            BezierConnectionManager.Instance.OnPortClicked(this);
        }
        // 右クリック: このポートに関係する接続をすべて削除
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            BezierConnectionManager.Instance.DisconnectPort(this);
        }
    }

    private void OnDestroy()
    {
        // InferenceBar の収縮などで Port が消えた場合、進行中の接続もキャンセルする
        if (BezierConnectionManager.Instance != null)
        {
            BezierConnectionManager.Instance.OnPortDestroyed(this);
        }
    }
}
