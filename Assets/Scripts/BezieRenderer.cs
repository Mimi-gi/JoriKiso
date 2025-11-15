using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer))]
public class BezieRenderer : MonoBehaviour
{
    [Header("Bezier Points (runtime updated)")]
    [SerializeField] private Vector3 startPoint;    // 始点
    [SerializeField] private Vector3 endPoint;      // 終点（暫定/確定）
    [SerializeField] private Vector3 controlPoint1; // 始点直下
    [SerializeField] private Vector3 controlPoint2; // 終点直上

    [Header("Settings")]
    [SerializeField] private int segmentCount = 30;      // 分割数
    [SerializeField] private float controlOffset = 1.0f; // 制御点オフセット距離
    [SerializeField] private float cancelClearDelay = 0f; // キャンセル時クリア遅延
    [SerializeField] private LayerMask portLayerMask = ~0; // Port 検出用レイヤ
    [SerializeField] private bool use2DPhysicsFirst = true; // 2D→3D の順でRaycast

    private LineRenderer lineRenderer;
    private Camera cam;

    // 状態管理
    private Port startPort;
    private Port endPort;
    private bool isDrawing = false;      // 始点確定後の第2クリック待ち
    private bool isFinalized = false;    // 終点確定済み
    private bool isDirty = false;        // 再描画要求

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        cam = Camera.main;
        ClearCurveImmediate();
    }

    void Update()
    {
        // Port の現在位置を常に参照してベジエ線を更新する
        if (!isDrawing)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        UpdateControlPoints();
        Redraw();
    }

    // 外部参照用ステート
    public bool IsDrawing => isDrawing;
    public bool IsFinalized => isFinalized;

    // ==== 公開API（外部から始点・終点管理）====
    public void StartCurve(Port port)
    {
        if (port == null) { Debug.LogWarning("StartCurve: port null"); return; }
        startPort = port;
        endPort = null;
        isDrawing = true;
        isFinalized = false;
        UpdateControlPoints();
    }

    // プレビュー用終点を任意座標で更新（確定前）
    public void PreviewEnd(Vector3 worldPosition)
    {
        if (!isDrawing || isFinalized) return;
        // プレビュー中は Port 参照ではなくカーソル位置を終点として使う
        endPort = null;
        endPoint = worldPosition;
        UpdateControlPoints();
    }

    // プレビュー用終点を Port 指定で更新（確定前）
    public void PreviewEnd(Port port)
    {
        if (port == null) return;
        endPort = port;
        UpdateControlPoints();
    }

    // 終点確定（Port）
    public void FinalizeCurve(Port port)
    {
        if (!isDrawing || port == null) return;
        endPort = port;
        isFinalized = true;
        UpdateControlPoints();
    }

    public void CancelCurve()
    {
        startPort = null;
        endPort = null;
        isDrawing = false;
        isFinalized = false;
        if (cancelClearDelay <= 0f) ClearCurveImmediate();
        else Invoke(nameof(ClearCurveImmediate), cancelClearDelay);
    }

    private void ClearCurveImmediate()
    {
        lineRenderer.positionCount = 0;
    }

    private void UpdateControlPoints()
    {
        // ポートがあれば常に最新位置を参照する
        if (startPort != null)
        {
            startPoint = startPort.transform.position;
        }
        if (endPort != null)
        {
            endPoint = endPort.transform.position;
        }

        controlPoint1 = new Vector3(startPoint.x, startPoint.y - controlOffset, startPoint.z);
        controlPoint2 = new Vector3(endPoint.x, endPoint.y + controlOffset, endPoint.z);
    }

    private void Redraw()
    {
        if (!isDrawing) { lineRenderer.positionCount = 0; return; }
        int count = Mathf.Max(1, segmentCount);
        var positions = new Vector3[count + 1];
        for (int i = 0; i <= count; i++)
        {
            float t = (float)i / count;
            positions[i] = Bezier3(startPoint, endPoint, controlPoint1, controlPoint2, t);
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private Vector3 Bezier3(Vector3 start, Vector3 end, Vector3 c1, Vector3 c2, float t)
    {
        Vector3 Q0 = Vector3.Lerp(start, c1, t);
        Vector3 Q1 = Vector3.Lerp(c1, c2, t);
        Vector3 Q2 = Vector3.Lerp(c2, end, t);
        Vector3 R0 = Vector3.Lerp(Q0, Q1, t);
        Vector3 R1 = Vector3.Lerp(Q1, Q2, t);
        return Vector3.Lerp(R0, R1, t);
    }

    private Vector3 GetMouseWorldPositionMatchingDepth(Vector3 reference)
    {
        if (cam == null) cam = Camera.main;
        var mp = Input.mousePosition;
        float z = cam.WorldToScreenPoint(reference).z;
        return cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, z));
    }

    // 外部からスクリーン座標を用いてプレビュー更新したい場合
    public void PreviewEndFromScreenPoint(Vector3 screenPoint)
    {
        if (!isDrawing || isFinalized) return;
        if (cam == null) cam = Camera.main;
        float z = cam.WorldToScreenPoint(startPoint).z;
        var world = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, z));
        PreviewEnd(world);
    }

    // 既存クリック判定は廃止。必要なら再度公開メソッド化して外部に委譲。
}
