using UnityEngine;

public class BezierConnectionManager : MonoBehaviour
{
    public static BezierConnectionManager Instance { get; private set; }

    [Header("Runtime Instantiation")] public Material lineMaterial;
    [Range(1,128)] public int segmentCount = 30;
    public float lineWidth = 0.05f;
    public float controlOffset = 1.0f;

    private BezieRenderer current;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    void Update()
    {
        // プレビュー更新: マウス位置で終点暫定表示
        if (current != null && current.IsDrawing && !current.IsFinalized)
        {
            current.PreviewEndFromScreenPoint(Input.mousePosition);
        }
    }

    public void OnPortClicked(Port port)
    {
        if (port == null) return;

        Debug.Log($"[BezierConnectionManager] Port clicked: {port.name}");

        // 新規開始条件: current が存在しない もしくは finalize 済 / drawing でない
        if (current == null || !current.IsDrawing || current.IsFinalized)
        {
            Debug.Log("[BezierConnectionManager] Start new curve.");
            CreateNewCurve(port);
            return;
        }

        // 2回目クリック: 異なるポートなら確定／同じポートなら何もしない
        if (current.IsDrawing && !current.IsFinalized)
        {
            // 開始ポートと異なるポートがクリックされたときだけ確定する
            if (Vector3.Distance(port.transform.position, GetStartPosition()) >= 0.0001f)
            {
                Debug.Log("[BezierConnectionManager] Finalize curve with another port.");
                current.FinalizeCurve(port);
            }
            else
            {
                Debug.Log("[BezierConnectionManager] Clicked start port again (ignored).");
            }
        }
    }

    // 指定ポートに関係する進行中の接続をキャンセルする
    public void DisconnectPort(Port port)
    {
        if (port == null) return;

        // 進行中の線があり、かつその始点がこのポートならキャンセルして削除
        if (current != null && current.IsDrawing && !current.IsFinalized)
        {
            if (Vector3.Distance(port.transform.position, GetStartPosition()) < 0.0001f)
            {
                current.CancelCurve();
                Destroy(current.gameObject);
                current = null;
            }
        }
    }

    // InferenceBar のリサイズなどで Port が破棄されたときにも接続をキャンセルする
    public void OnPortDestroyed(Port port)
    {
        if (port == null) return;

        if (current != null && current.IsDrawing && !current.IsFinalized)
        {
            // 始点ポートが消えた場合はただちにキャンセル
            if (Vector3.Distance(port.transform.position, GetStartPosition()) < 0.0001f)
            {
                current.CancelCurve();
                Destroy(current.gameObject);
                current = null;
            }
        }
    }

    private void CreateNewCurve(Port startPort)
    {
        // GameObject生成
        var go = new GameObject("BezierConnection");
        go.transform.SetParent(this.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.widthMultiplier = lineWidth;
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;

        current = go.AddComponent<BezieRenderer>();
        // 初期設定: segmentCount と controlOffset を反映
        SetPrivateField(current, "segmentCount", segmentCount);
        SetPrivateField(current, "controlOffset", controlOffset);
        current.StartCurve(startPort);
    }

    private Vector3 GetStartPosition()
    {
        // 反射で startPoint 取得（公開プロパティにしたければ BezieRenderer へ追加）
        if (current == null) return Vector3.zero;
        var f = typeof(BezieRenderer).GetField("startPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) return (Vector3)f.GetValue(current);
        return Vector3.zero;
    }

    private void SetPrivateField(object obj, string name, object value)
    {
        var f = obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null)
        {
            f.SetValue(obj, value);
        }
    }
}