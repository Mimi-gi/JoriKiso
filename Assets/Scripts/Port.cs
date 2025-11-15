using UnityEngine;
using UnityEngine.EventSystems;

// UI 上のポート。クリックイベント(IPointerClickHandler)で接続マネージャへ通知する。
public class Port : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SequentNode parentNode;
    public SequentNode ParentNode => parentNode;
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
