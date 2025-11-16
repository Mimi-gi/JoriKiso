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
    private SequentNode draggedNode = null;

    public Sequent Sequent { get; private set; }
    
    // 推論チェーン構築用：前後の InferenceBar を記録
    public InferenceBar PreviousInferenceBar { get; set; }
    public InferenceBar NextInferenceBar { get; set; }

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
        Debug.Log("SequentNode Begin Drag - Creating duplicate");
        
        // InferenceSuggestionPanel の content 内にあるかチェック
        if (InferenceSuggestionPanel.Instance != null)
        {
            var nodeRect = GetComponent<RectTransform>();
            if (InferenceSuggestionPanel.Instance.IsNodeInContent(nodeRect))
            {
                Debug.Log("SequentNode is in InferenceSuggestionPanel content - creating duplicate");
                
                // 複製を作成し、マウス位置に配置
                draggedNode = DuplicateSequentNodeAtPosition(eventData);
                if (draggedNode != null)
                {
                    // Pointer に複製を登録
                    if (Pointer.Instance != null)
                    {
                        Pointer.Instance.Register(draggedNode);
                    }
                    Debug.Log("Duplicate SequentNode created and registered to Pointer");
                }
                return;
            }
        }
        
        // 通常のドラッグの場合（複製ではない）
        draggedNode = this;
        if (Pointer.Instance != null)
        {
            Pointer.Instance.Register(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ドラッグ対象を取得
        SequentNode targetNode = draggedNode != null ? draggedNode : this;
        RectTransform targetRT = targetNode.GetComponent<RectTransform>();
        
        if (targetRT == null) return;

        // draggedNode（複製）の場合、Canvas座標で計算
        if (draggedNode != null && draggedNode == targetNode)
        {
            var canvasRect = CanvasRect.Main;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                targetRT.anchoredPosition = localPoint;
            }
            else
            {
                targetRT.position = eventData.position;
            }
            return;
        }

        // 通常のドラッグの場合
        var parentRT = targetRT.parent as RectTransform;
        Vector2 localPoint2;
        if (parentRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, eventData.position, eventData.pressEventCamera, out localPoint2))
        {
            targetRT.anchoredPosition = localPoint2;
        }
        else
        {
            targetRT.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("SequentNode End Drag");
        
        if (draggedNode != null && draggedNode != this)
        {
            draggedNode = null;
        }
        else if (Pointer.Instance != null)
        {
            Pointer.Instance.Unregister();
        }
    }

    /// <summary>
    /// SequentNode の複製を作成する。
    /// Instantiate でクローンを作成し、Canvas に配置する。
    /// </summary>
    private SequentNode DuplicateSequentNode()
    {
        SequentNode duplicated = Instantiate(gameObject).GetComponent<SequentNode>();
        if (duplicated == null) return null;

        // Canvas に配置
        var duplicatedRT = duplicated.GetComponent<RectTransform>();
        var originalRT = GetComponent<RectTransform>();
        
        duplicatedRT.SetParent(CanvasRect.Main, false);
        duplicatedRT.anchoredPosition = originalRT.anchoredPosition;

        Debug.Log("[SequentNode.DuplicateSequentNode] Duplicated SequentNode created");
        return duplicated;
    }

    /// <summary>
    /// SequentNode の複製を作成し、マウス位置に配置する。
    /// 複製元のノード構造を解析してSequentを取得し、そこから新しいノードを生成する。
    /// </summary>
    private SequentNode DuplicateSequentNodeAtPosition(PointerEventData eventData)
    {
        Debug.Log("[DuplicateSequentNodeAtPosition] Starting duplication by analyzing source structure");
        
        // ステップ 1: 複製元から Sequent を取得
        if (!TryGenerateSequentFromFrames(out var sourceSequent))
        {
            Debug.LogError("[DuplicateSequentNodeAtPosition] Failed to generate Sequent from source frames");
            return null;
        }
        
        Debug.Log($"[DuplicateSequentNodeAtPosition] Source Sequent: Antecedents={sourceSequent.Antecedents.Count}, Consequents={sourceSequent.Consequents.Count}");

        // ステップ 2: このノード（プレハブ参照）を複製してから中身をクリア
        var duplicated = Instantiate(gameObject, CanvasRect.Main).GetComponent<SequentNode>();
        if (duplicated == null)
        {
            Debug.LogError("[DuplicateSequentNodeAtPosition] Failed to instantiate and get SequentNode");
            return null;
        }

        Debug.Log("[DuplicateSequentNodeAtPosition] Base prefab instantiated");

        // ステップ 3: 複製されたノードのフレームをすべてクリア
        duplicated.RightFrames.Clear();
        duplicated.LeftFrames.Clear();

        // 既存のフレームゲームオブジェクトを削除
        for (int i = duplicated.transform.childCount - 1; i >= 0; i--)
        {
            var child = duplicated.transform.GetChild(i);
            var frame = child.GetComponent<Frame>();
            if (frame != null)
            {
                Destroy(child.gameObject);
            }
        }

        Debug.Log("[DuplicateSequentNodeAtPosition] Cleared existing frames");

        // ステップ 4: NodeCreator を使ってフレームとノードを生成
        // Antecedents（左側）
        for (int i = 0; i < sourceSequent.Antecedents.Count; i++)
        {
            duplicated.AddLeftFrame();
            var frame = duplicated.LeftFrames[duplicated.LeftFrames.Count - 1];
            if (frame == null)
            {
                Debug.LogError($"[DuplicateSequentNodeAtPosition] Failed to add left frame {i}");
                Destroy(duplicated.gameObject);
                return null;
            }

            Debug.Log($"[DuplicateSequentNodeAtPosition] Creating node from formula: {sourceSequent.Antecedents[i]}");
            var node = NodeCreator.CreateNodeFromFormula(sourceSequent.Antecedents[i], frame.RectTransform);
            if (node == null)
            {
                Debug.LogError($"[DuplicateSequentNodeAtPosition] Failed to create node for antecedent {i}");
                Destroy(duplicated.gameObject);
                return null;
            }

            frame.SetNodeDirect(node);
        }

        // Consequents（右側）
        for (int i = 0; i < sourceSequent.Consequents.Count; i++)
        {
            duplicated.AddRightFrame();
            var frame = duplicated.RightFrames[duplicated.RightFrames.Count - 1];
            if (frame == null)
            {
                Debug.LogError($"[DuplicateSequentNodeAtPosition] Failed to add right frame {i}");
                Destroy(duplicated.gameObject);
                return null;
            }

            Debug.Log($"[DuplicateSequentNodeAtPosition] Creating node from formula: {sourceSequent.Consequents[i]}");
            var node = NodeCreator.CreateNodeFromFormula(sourceSequent.Consequents[i], frame.RectTransform);
            if (node == null)
            {
                Debug.LogError($"[DuplicateSequentNodeAtPosition] Failed to create node for consequent {i}");
                Destroy(duplicated.gameObject);
                return null;
            }

            frame.SetNodeDirect(node);
        }

        // ステップ 5: マウス位置に配置
        var duplicatedRT = duplicated.GetComponent<RectTransform>();
        duplicatedRT.anchorMin = new Vector2(0.5f, 0.5f);
        duplicatedRT.anchorMax = new Vector2(0.5f, 0.5f);
        
        var canvasRect = CanvasRect.Main;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            duplicatedRT.anchoredPosition = localPoint;
        }
        else
        {
            duplicatedRT.position = eventData.position;
        }

        Debug.Log($"[DuplicateSequentNodeAtPosition] Duplication complete at position {duplicatedRT.anchoredPosition}");
        
        // ステップ 6: Sequent を生成して確認
        if (duplicated.TryGenerateSequentFromFrames(out var duplicatedSequent))
        {
            Debug.Log($"[DuplicateSequentNodeAtPosition] Duplicated Sequent verified: Antecedents={duplicatedSequent.Antecedents.Count}, Consequents={duplicatedSequent.Consequents.Count}");
        }
        else
        {
            Debug.LogWarning("[DuplicateSequentNodeAtPosition] Failed to verify duplicated Sequent");
        }
        
        return duplicated;
    }

    // RectTransform の横幅(sizeDelta.x)をキャッシュして変化を検出
    private List<float> rightLastWidths = new List<float>();
    private List<float> leftLastWidths = new List<float>();

    void Update()
    {
        // ボタン位置を常に再計算
        RecalculateRightButtonPositions();
        RecalculateLeftButtonPositions();

        bool needRight = false;
        if (rightLastWidths.Count != RightFrames.Count) 
        {
            Debug.Log($"[Update] RightFrames count changed: {rightLastWidths.Count} -> {RightFrames.Count}");
            needRight = true;
        }
        else
        {
            for (int i = 0; i < RightFrames.Count; i++)
            {
                var f = RightFrames[i];
                if (f == null || f.RectTransform == null) { needRight = true; break; }
                var w = f.RectTransform.sizeDelta.x;
                if (!Mathf.Approximately(w, rightLastWidths[i])) 
                { 
                    Debug.Log($"[Update] RightFrame {i} width changed: {rightLastWidths[i]} -> {w}");
                    needRight = true; 
                    break; 
                }
            }
        }
        if (needRight) RecalculateRightPositions();

        bool needLeft = false;
        if (leftLastWidths.Count != LeftFrames.Count) 
        {
            Debug.Log($"[Update] LeftFrames count changed: {leftLastWidths.Count} -> {LeftFrames.Count}");
            needLeft = true;
        }
        else
        {
            for (int i = 0; i < LeftFrames.Count; i++)
            {
                var f = LeftFrames[i];
                if (f == null || f.RectTransform == null) { needLeft = true; break; }
                var w = f.RectTransform.sizeDelta.x;
                if (!Mathf.Approximately(w, leftLastWidths[i])) 
                { 
                    Debug.Log($"[Update] LeftFrame {i} width changed: {leftLastWidths[i]} -> {w}");
                    needLeft = true; 
                    break; 
                }
            }
        }
        if (needLeft) RecalculateLeftPositions();

        // すべてのフレームが埋まり、各ノードのFormulaが妥当なら Sequent を生成
        TryGenerateSequentFromFrames(out _);
    }

    /// <summary>
    /// 右側ボタン位置を再計算して更新する。
    /// </summary>
    private void RecalculateRightButtonPositions()
    {
        // 末端フレームに続けてボタン位置を配置
        if (RightFrames.Count > 0)
        {
            // フレームの Width の総和を計算
            float totalWidth = 0f;
            for (int i = 0; i < RightFrames.Count; i++)
            {
                float frameWidth = RightFrames[i].RectTransform.sizeDelta.x;
                totalWidth += frameWidth;
            }
            
            Vector2 basePos = new Vector2(7f + totalWidth, 0f);
            rightUpButton.anchoredPosition = basePos;
            rightDownButton.anchoredPosition = basePos;
        }
        else
        {
            // フレームが無い場合は初期位置付近に配置
            rightUpButton.anchoredPosition = new Vector2(7f, 0f);
            rightDownButton.anchoredPosition = new Vector2(7f, 0f);
        }
    }

    /// <summary>
    /// 左側ボタン位置を再計算して更新する。
    /// </summary>
    private void RecalculateLeftButtonPositions()
    {
        // 末端（最も左側）フレームのさらに左へボタン配置
        if (LeftFrames.Count > 0)
        {
            // フレームの Width の総和を計算
            float totalWidth = 0f;
            for (int i = 0; i < LeftFrames.Count; i++)
            {
                float frameWidth = LeftFrames[i].RectTransform.sizeDelta.x;
                totalWidth += frameWidth;
            }
            
            Vector2 basePos = new Vector2(-7f - totalWidth, 0f);
            leftUpButton.anchoredPosition = basePos;
            leftDownButton.anchoredPosition = basePos;
        }
        else
        {
            leftUpButton.anchoredPosition = new Vector2(-7f, 0f);
            leftDownButton.anchoredPosition = new Vector2(-7f, 0f);
        }
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
                    float offsetX = prev.Length; // Frame.Length を使用
                    rt.anchoredPosition = prevRT.anchoredPosition + new Vector2(offsetX, 0f);
                }
            }
            rightLastWidths.Add(rt.sizeDelta.x);
        }
    }

    // 現在の Left/RightFrames から Sequent 生成を試みる
    // - Filled なフレームから得られる Formula だけを集めて生成
    // - 片側が空でも（もう一方に式があれば）生成を試みる
    // - 成功時は this.Sequent を更新し true を返す
    public bool TryGenerateSequentFromFrames(out Sequent result)
    {
        result = default;
        if (LeftFrames == null || RightFrames == null) return false;
        var left = new List<Formula>();
        var right = new List<Formula>();

        // 左側収集: Filled なフレームだけを式として採用し、それ以外はスキップ
        for (int i = 0; i < LeftFrames.Count; i++)
        {
            var frame = LeftFrames[i];
            if (TryGetFormulaFromFrame(frame, out var formula))
            {
                left.Add(formula);
            }
            else
            {
            }
        }

        // 右側収集: 同様に、取れる式だけを集める
        for (int i = 0; i < RightFrames.Count; i++)
        {
            var frame = RightFrames[i];
            if (TryGetFormulaFromFrame(frame, out var formula))
            {
                right.Add(formula);
            }
            else
            {
            }
        }

        // 左右どちらにも式が 1 つも無い場合は Sequent を生成しない
        if (left.Count == 0 && right.Count == 0)
        {
            return false;
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

    // Sequent の生成（Formulas に包んでそのままコンストラクタ呼び出し）
    private bool TryCreateSequent(List<Formula> left, List<Formula> right, out Sequent sequent)
    {
        try
        {
            var antecedents = new Formulas(left);
            var consequents = new Formulas(right);
            sequent = new Sequent(antecedents, consequents);
            return true;
        }
        catch
        {
            sequent = default;
            return false;
        }
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
                    float offsetX = prev.Length; // Frame.Length を使用
                    // 左方向へ伸ばす: 前フレームの位置から幅分マイナス
                    lrt.anchoredPosition = prev.RectTransform.anchoredPosition - new Vector2(offsetX, 0f);
                }
            }
            leftLastWidths.Add(lrt.sizeDelta.x);
        }
    }

    // 右側フレームを1つ追加
    public void AddRightFrame()
    {
        if (rightframePrefab == null) 
        {
            Debug.LogError("[SequentNode.AddRightFrame] rightframePrefab is null");
            return;
        }
        // 配置コンテナは up ボタンの親を仮定（必要なら別シリアライズ参照に変更）
        Transform parentContainer = rightUpButton != null ? rightUpButton.parent : this.transform;
        var go = Instantiate(rightframePrefab, parentContainer);
        var frame = go.GetComponent<Frame>();
        if (frame == null)
        {
            Debug.LogError("[SequentNode.AddRightFrame] Frame component not found on instantiated prefab");
            Destroy(go);
            return;
        }
        Debug.Log("[SequentNode.AddRightFrame] RightFrame added: " + go.name);
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
        if (leftframePrefab == null) 
        {
            Debug.LogError("[SequentNode.AddLeftFrame] leftframePrefab is null");
            return;
        }
        Transform parentContainer = leftUpButton != null ? leftUpButton.parent : this.transform;
        var go = Instantiate(leftframePrefab, parentContainer);
        var frame = go.GetComponent<Frame>();
        if (frame == null)
        {
            Debug.LogError("[SequentNode.AddLeftFrame] Frame component not found on instantiated prefab");
            Destroy(go);
            return;
        }
        Debug.Log("[SequentNode.AddLeftFrame] LeftFrame added: " + go.name);
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
