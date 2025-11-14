using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using FormalSystem.LK;
using R3;
using R3.Collections;
using ObservableCollections;

public class SequentNode : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ObservableList<Frame> RightFrames = new ObservableList<Frame>();
    public ObservableList<Frame> LeftFrames = new ObservableList<Frame>();
    [SerializeField] RectTransform rightUpButton; //+
    [SerializeField] RectTransform rightDownButton; //-
    [SerializeField] RectTransform leftUpButton; //+
    [SerializeField] RectTransform leftDownButton; //-
    [SerializeField] GameObject leftframePrefab;
    [SerializeField] GameObject rightframePrefab;

    private RectTransform rt;

    public Sequent Sequent { get; private set; }

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        //スタートは前件と後件両方空から始める。フレームがReplaceされるごとに描画しなおす
        // 初期レイアウト
        RecalculateRightPositions();
        RecalculateLeftPositions();
    }

    // SequentNode 自体のドラッグハンドラ（ボタン等は固定）
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 必要ならドラッグ開始時の処理を追加
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rt == null) return;
        var parentRT = rt.parent as RectTransform;
        Vector2 localPoint;
        if (parentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            rt.anchoredPosition = localPoint;
        }
        else
        {
            rt.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 必要ならドラッグ終了時の処理を追加
    }

    // RectTransform の横幅(sizeDelta.x)をキャッシュして変化を検出
    private List<float> rightLastWidths = new List<float>();
    private List<float> leftLastWidths = new List<float>();

    void Update()
    {
        bool needRight = false;
        if (rightLastWidths.Count != RightFrames.Count) needRight = true; else
        {
            for (int i = 0; i < RightFrames.Count; i++)
            {
                var f = RightFrames[i];
                if (f == null || f.RectTransform == null) { needRight = true; break; }
                var w = f.RectTransform.sizeDelta.x;
                if (!Mathf.Approximately(w, rightLastWidths[i])) { needRight = true; break; }
            }
        }
        if (needRight) RecalculateRightPositions();

        bool needLeft = false;
        if (leftLastWidths.Count != LeftFrames.Count) needLeft = true; else
        {
            for (int i = 0; i < LeftFrames.Count; i++)
            {
                var f = LeftFrames[i];
                if (f == null || f.RectTransform == null) { needLeft = true; break; }
                var w = f.RectTransform.sizeDelta.x;
                if (!Mathf.Approximately(w, leftLastWidths[i])) { needLeft = true; break; }
            }
        }
        if (needLeft) RecalculateLeftPositions();

        // すべてのフレームが埋まり、各ノードのFormulaが妥当なら Sequent を生成
        TryGenerateSequentFromFrames(out _);
    }

    private void RecalculateRightPositions()
    {
        rightLastWidths.Clear();
        for (int i = 0; i < RightFrames.Count; i++)
        {
            var frame = RightFrames[i];
            if (frame == null || frame.RectTransform == null) continue;
            var rt = frame.RectTransform;
            if (i == 0)
            {
                rt.anchoredPosition = new Vector2(7f, 0f);
            }
            else
            {
                var prev = RightFrames[i - 1];
                if (prev != null && prev.RectTransform != null)
                {
                    var prevRT = prev.RectTransform;
                    float offsetX = prevRT.sizeDelta.x; // 直前フレームの幅分だけずらす
                    rt.anchoredPosition = prevRT.anchoredPosition + new Vector2(offsetX, 0f);
                }
            }
            rightLastWidths.Add(rt.sizeDelta.x);
        }

        // 末端フレームに続けてボタン位置を再配置
        if (RightFrames.Count > 0)
        {
            var last = RightFrames[RightFrames.Count - 1];
            if (last != null && last.RectTransform != null)
            {
                var lastRT = last.RectTransform;
                Vector2 basePos = lastRT.anchoredPosition + new Vector2(lastRT.sizeDelta.x, 0f);
                if (rightUpButton != null) rightUpButton.anchoredPosition = basePos;
                if (rightDownButton != null) rightDownButton.anchoredPosition = basePos; // 下に少しずらす例（必要なら調整）
            }
        }
        else
        {
            // フレームが無い場合は初期位置付近に配置
            if (rightUpButton != null) rightUpButton.anchoredPosition = new Vector2(7f, 0f);
            if (rightDownButton != null) rightDownButton.anchoredPosition = new Vector2(7f, 0f);
        }
    }

    // 現在の Left/RightFrames から Sequent 生成を試みる
    // - すべてのフレームが埋まっており、各ノードの Formula が null でない場合に生成
    // - 成功時は this.Sequent を更新し true を返す
    public bool TryGenerateSequentFromFrames(out Sequent result)
    {
        result = default;
        if (LeftFrames == null || RightFrames == null) return false;

        var left = new List<Formula>();
        var right = new List<Formula>();

        // 左側収集
        for (int i = 0; i < LeftFrames.Count; i++)
        {
            var frame = LeftFrames[i];
            if (!TryGetFormulaFromFrame(frame, out var formula)) return false;
            left.Add(formula);
        }

        // 右側収集
        for (int i = 0; i < RightFrames.Count; i++)
        {
            var frame = RightFrames[i];
            if (!TryGetFormulaFromFrame(frame, out var formula)) return false;
            right.Add(formula);
        }

        if (TryCreateSequent(left, right, out var seq))
        {
            Sequent = seq;
            result = seq;
            return true;
        }
        return false;
    }

    // Frame から Formula を取得する。以下を前提（リフレクションで緩く対応）
    // - Frame に Filled/IsFilled(bool) がある場合、それが true であること
    // - Frame に Node/CurrentNode/Content のいずれかのプロパティがあり、
    //   そこから Formula プロパティが取得でき、null でないこと
    private bool TryGetFormulaFromFrame(object frameObj, out Formula formula)
    {
        formula = default;
        if (frameObj == null) return false;

        var ft = frameObj.GetType();
        // Filled 判定（存在しなければスキップ）
        var filledProp = ft.GetProperty("Filled") ?? ft.GetProperty("IsFilled");
        if (filledProp != null)
        {
            var filledVal = filledProp.GetValue(frameObj);
            if (filledVal is bool b && !b) return false;
        }

        // ノード取得
        var nodeProp = ft.GetProperty("Node") ?? ft.GetProperty("CurrentNode") ?? ft.GetProperty("Content");
        if (nodeProp == null) return false;
        var node = nodeProp.GetValue(frameObj);
        if (node == null) return false;

        // Formula 取得
        var nt = node.GetType();
        var formulaProp = nt.GetProperty("Formula");
        if (formulaProp == null) return false;
        var val = formulaProp.GetValue(node);
        if (val == null) return false;

        formula = (Formula)val;
        return true;
    }

    // Sequent の生成（2引数コンストラクタのみをリフレクションで探索）
    private bool TryCreateSequent(List<Formula> left, List<Formula> right, out Sequent sequent)
    {
        sequent = default;
        try
        {
            var st = typeof(Sequent);

            // 2引数コンストラクタのみ採用（List/Enumerable 等に対応）
            foreach (var ctor in st.GetConstructors())
            {
                var ps = ctor.GetParameters();
                if (ps.Length != 2) continue;

                bool p0ok = ps[0].ParameterType.IsInstanceOfType(left) || ps[0].ParameterType.IsAssignableFrom(typeof(List<Formula>)) || ps[0].ParameterType.IsAssignableFrom(typeof(IEnumerable<Formula>));
                bool p1ok = ps[1].ParameterType.IsInstanceOfType(right) || ps[1].ParameterType.IsAssignableFrom(typeof(List<Formula>)) || ps[1].ParameterType.IsAssignableFrom(typeof(IEnumerable<Formula>));
                if (p0ok && p1ok)
                {
                    var obj = ctor.Invoke(new object[] { left, right });
                    sequent = (Sequent)obj;
                    return true;
                }
            }
        }
        catch
        {
            // 生成失敗時は false
        }
        return false;
    }

    private void RecalculateLeftPositions()
    {
        leftLastWidths.Clear();
        for (int i = 0; i < LeftFrames.Count; i++)
        {
            var frame = LeftFrames[i];
            if (frame == null || frame.RectTransform == null) continue;
            var lrt = frame.RectTransform;
            if (i == 0)
            {
                lrt.anchoredPosition = new Vector2(-7f, 0f);
            }
            else
            {
                var prev = LeftFrames[i - 1];
                if (prev != null && prev.RectTransform != null)
                {
                    var prevRT = prev.RectTransform;
                    float offsetX = prevRT.sizeDelta.x;
                    // 左方向へ伸ばす: 前フレームの位置から幅分マイナス
                    lrt.anchoredPosition = prevRT.anchoredPosition - new Vector2(offsetX, 0f);
                }
            }
            leftLastWidths.Add(lrt.sizeDelta.x);
        }

        // 末端（最も左側）フレームのさらに左へボタン配置
        if (LeftFrames.Count > 0)
        {
            var last = LeftFrames[LeftFrames.Count - 1];
            if (last != null && last.RectTransform != null)
            {
                var lastRT = last.RectTransform;
                Vector2 basePos = lastRT.anchoredPosition - new Vector2(lastRT.sizeDelta.x, 0f);
                if (leftUpButton != null) leftUpButton.anchoredPosition = basePos;
                if (leftDownButton != null) leftDownButton.anchoredPosition = basePos; // y は 0 維持
            }
        }
        else
        {
            if (leftUpButton != null) leftUpButton.anchoredPosition = new Vector2(-7f, 0f);
            if (leftDownButton != null) leftDownButton.anchoredPosition = new Vector2(-7f, 0f);
        }
    }

    // 右側フレームを1つ追加
    public void AddRightFrame()
    {
        if (rightframePrefab == null) return;
        // 配置コンテナは up ボタンの親を仮定（必要なら別シリアライズ参照に変更）
        Transform parentContainer = rightUpButton != null ? rightUpButton.parent : this.transform;
        var go = Instantiate(rightframePrefab, parentContainer);
        var frame = go.GetComponent<Frame>();
        if (frame == null)
        {
            Destroy(go);
            return;
        }
        RightFrames.Add(frame);
        RecalculateRightPositions();
    }

    // 右側フレームを末尾から1つ削除
    public void RemoveRightFrame()
    {
        if (RightFrames.Count == 0) return;
        var frame = RightFrames[RightFrames.Count - 1];
        RightFrames.RemoveAt(RightFrames.Count - 1);
        if (frame != null) Destroy(frame.gameObject);
        RecalculateRightPositions();
    }

    // 左側フレームを1つ追加
    public void AddLeftFrame()
    {
        if (leftframePrefab == null) return;
        Transform parentContainer = leftUpButton != null ? leftUpButton.parent : this.transform;
        var go = Instantiate(leftframePrefab, parentContainer);
        var frame = go.GetComponent<Frame>();
        if (frame == null)
        {
            Destroy(go);
            return;
        }
        LeftFrames.Add(frame);
        RecalculateLeftPositions();
    }

    // 左側フレームを末尾（さらに左側）から1つ削除
    public void RemoveLeftFrame()
    {
        if (LeftFrames.Count == 0) return;
        var frame = LeftFrames[LeftFrames.Count - 1];
        LeftFrames.RemoveAt(LeftFrames.Count - 1);
        if (frame != null) Destroy(frame.gameObject);
        RecalculateLeftPositions();
    }
}
