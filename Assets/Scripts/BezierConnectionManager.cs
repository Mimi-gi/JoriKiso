using UnityEngine;

public class BezierConnectionManager : MonoBehaviour
{
    public static BezierConnectionManager Instance { get; private set; }

    [Header("Runtime Instantiation")] public Material lineMaterial;
    [Range(1,128)] public int segmentCount = 30;
    public float lineWidth = 0.05f;
    public float controlOffset = 1.0f;

    // 進行中の接続（ドラッグ中は1本のみを扱う）
    private BezieRenderer current;

    // 確定済みの接続一覧（右クリック削除やポート破棄対応用）
    private readonly System.Collections.Generic.List<BezieRenderer> finalized = new System.Collections.Generic.List<BezieRenderer>();

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

        // 2回目クリック: 異なるポートなら確定／同じポートならキャンセル
        if (current.IsDrawing && !current.IsFinalized)
        {
            float dist = Vector3.Distance(port.transform.position, GetStartPosition());
            Debug.Log($"[BezierConnectionManager] Second click. Port={port.name}, dist={dist}");

            // ① 開始ポートと異なるポート: 接続確定
            if (dist >= 0.0001f)
            {
                Debug.Log("[BezierConnectionManager] Finalize curve with another port.");
                // 接続確定
                current.FinalizeCurve(port);

                // 接続に関わるポートを取得
                var startPort = GetPortFromBezier(current, true);
                var endPort = port;

                // InferenceBar 側のポートには ParentNode が無いので、
                // 接続先（start / end）両方の Port に、相手側の SequentNode を親として紐付ける。
                if (startPort != null && endPort != null)
                {
                    if (startPort.ParentNode == null && endPort.ParentNode != null)
                    {
                        startPort.SetParentNode(endPort.ParentNode);
                    }
                    else if (endPort.ParentNode == null && startPort.ParentNode != null)
                    {
                        endPort.SetParentNode(startPort.ParentNode);
                    }

                    // どちらも「現在接続あり」とマーク
                    startPort.IsConnected = true;
                    endPort.IsConnected = true;
                }

                // 確定済みリストに移動
                finalized.Add(current);
                current = null;
                return;
            }

            // ② 開始ポートと同じポート: 現在の線をキャンセル
            Debug.Log("[BezierConnectionManager] Clicked start port again -> cancel current curve.");
            current.CancelCurve();
            Destroy(current.gameObject);
            current = null;
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

        // 確定済みの線のうち、このポートを始点/終点に持つものを削除
        for (int i = finalized.Count - 1; i >= 0; i--)
        {
            var bez = finalized[i];
            if (bez == null)
            {
                finalized.RemoveAt(i);
                continue;
            }

            var start = GetPortFromBezier(bez, true);
            var end = GetPortFromBezier(bez, false);
            if (start == port || end == port)
            {
                // 接続状態フラグを更新
                if (start != null) start.IsConnected = false;
                if (end != null) end.IsConnected = false;

                // InferenceBar 側の ParentNode をクリア（シーケント側は保持）
                ClearInferenceBarSideParent(start, end);

                bez.CancelCurve();
                Destroy(bez.gameObject);
                finalized.RemoveAt(i);
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

        // 確定済みの接続も、破棄されたポートを含むものは削除
        for (int i = finalized.Count - 1; i >= 0; i--)
        {
            var bez = finalized[i];
            if (bez == null)
            {
                finalized.RemoveAt(i);
                continue;
            }

            var start = GetPortFromBezier(bez, true);
            var end = GetPortFromBezier(bez, false);
            if (start == port || end == port)
            {
                // 接続状態フラグを更新
                if (start != null) start.IsConnected = false;
                if (end != null) end.IsConnected = false;

                // InferenceBar 側の ParentNode をクリア（シーケント側は保持）
                ClearInferenceBarSideParent(start, end);
                bez.CancelCurve();
                Destroy(bez.gameObject);
                finalized.RemoveAt(i);
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

    // 反射で BezieRenderer に保持されている Port 参照を取得
    private Port GetPortFromBezier(BezieRenderer bez, bool start)
    {
        if (bez == null) return null;
        var fieldName = start ? "startPort" : "endPort";
        var f = typeof(BezieRenderer).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return f != null ? f.GetValue(bez) as Port : null;
    }

    // 接続解除時に、元々 ParentNode が null だった側（= InferenceBar 側）だけを null に戻す
    private void ClearInferenceBarSideParent(Port a, Port b)
    {
        if (a == null || b == null) return;

        // 「もともと null だった側」を null に戻したいので、
        // 現在 null でない方はシーケント側だとみなし、null 側だけクリアする。
        if (a.ParentNode == null && b.ParentNode != null)
        {
            a.SetParentNode(null);
        }
        else if (b.ParentNode == null && a.ParentNode != null)
        {
            b.SetParentNode(null);
        }
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