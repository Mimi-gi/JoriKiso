using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FormalSystem.LK;
using System.Linq;

/// <summary>
/// 推論サポート用のスクロールビューコンテンツを管理するシングルトン。
/// シーンに 1 つだけ配置し、LastInferenceBar に応じて内容をリフレッシュする。
/// </summary>
public class InferenceSuggestionPanel : MonoBehaviour
{
    public static InferenceSuggestionPanel Instance { get; private set; }

    [Header("Scroll View Content")]
    [SerializeField] private RectTransform content;

    [Header("Prefabs for Suggestions")]
    [SerializeField] private SequentNode sequentNodePrefab; // ② 結論シーケント表示用

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 外部から明示的に content を設定したい場合用。
    /// </summary>
    public void SetContent(RectTransform contentTransform)
    {
        content = contentTransform;
    }

    /// <summary>
    /// 現在 Pointer に記録されている LastInferenceBar に基づいて内容を更新する。
    /// </summary>
    public void RefreshFromPointer()
    {
        if (Pointer.Instance == null || Pointer.Instance.LastInferenceBar == null)
        {
            Debug.Log("[InferenceSuggestionPanel] No LastInferenceBar to refresh from.");
            Clear();
            return;
        }
        RefreshFor(Pointer.Instance.LastInferenceBar);
    }

    /// <summary>
    /// 指定された InferenceBar をもとに内容を更新する。
    /// isStart=true の場合は A ⊢ A をサジェストする。
    /// それ以外の場合、前提+ルール → 結論、または前提+結論 → ルールをサジェストする。
    /// </summary>
    public void RefreshFor(InferenceBar bar)
    {
        if (bar == null)
        {
            Clear();
            return;
        }

        Clear();
        Debug.Log($"[InferenceSuggestionPanel] Refresh for InferenceBar: {bar.name}");
        if (content == null)
        {
            Debug.LogError("[InferenceSuggestionPanel] content is not assigned.");
            return;
        }

        // スタートバーの場合：A ⊢ A をサジェスト
        if (bar.isStart)
        {
            TryShowStartBarSuggestion(bar);
            return;
        }

        // ②: 前提 + ルールノード が埋まっている場合、結論候補 SequentNode を列挙
        TryShowConclusionsFromPremisesAndRule(bar);

        // ③: 前提 + 結論シーケント が埋まっている場合、適用可能ルール候補 RuleNode を列挙
        TryShowRulesFromPremisesAndConclusion(bar);
    }

    /// <summary>
    /// スタートバーの場合、A ⊢ A の形式のシーケントをサジェストする。
    /// A は任意の原子式なので、サジェストとしていくつかの例を表示する。
    /// </summary>
    private void TryShowStartBarSuggestion(InferenceBar bar)
    {
        if (sequentNodePrefab == null)
        {
            Debug.Log("[InferenceSuggestionPanel] sequentNodePrefab is not assigned. Skip start bar suggestion.");
            return;
        }

        Debug.Log("[InferenceSuggestionPanel] Showing start bar suggestion: A ⊢ A");

        // スタートバー用のサジェスト：A, B, C の単純な変数で A ⊢ A を作成
        var suggestions = new List<(string label, Formula formula)>
        {
            ("A ⊢ A", new Variable(Alphabet.A)),
            ("B ⊢ B", new Variable(Alphabet.B)),
            ("C ⊢ C", new Variable(Alphabet.C)),
        };

        int created = 0;
        foreach (var (label, formula) in suggestions)
        {
            // A ⊢ A の形式でシーケントを作成
            var sequent = new Sequent(
                new Formulas(new List<Formula> { formula }),
                new Formulas(new List<Formula> { formula })
            );

            Debug.Log($"[InferenceSuggestionPanel] Creating SequentNode for start bar suggestion: {label}");
            var node = NodeCreator.CreateSequentNode(sequent, sequentNodePrefab, content);
            if (node == null)
            {
                Debug.LogWarning($"[InferenceSuggestionPanel] Failed to create SequentNode for {label}");
                continue;
            }

            PreparePreviewSequentNode(node);
            created++;
        }

        Debug.Log($"[InferenceSuggestionPanel] Total start bar suggestions created: {created}");
        AdjustContentHeight(created);
    }

    /// <summary>
    /// ② 前提 + ルールノード から結論候補を生成して content に追加する。
    /// </summary>
    private void TryShowConclusionsFromPremisesAndRule(InferenceBar bar)
    {
        if (sequentNodePrefab == null)
        {
            Debug.Log("[InferenceSuggestionPanel] sequentNodePrefab is not assigned. Skip ②.");
            return;
        }

        // 前提: InferenceBar の PremisesPorts それぞれに SequentNode が割り当てられているとみなして収集
        var premises = new List<Sequent>();
        Debug.Log($"[InferenceSuggestionPanel] ② Collect premises from {bar.PremisesPorts.Count} premise ports.");
        foreach (var port in bar.PremisesPorts)
        {
            if (port == null)
            {
                Debug.Log("[InferenceSuggestionPanel] ② Found null port in PremisesPorts. Skip.");
                continue;
            }
            if (port.ParentNode == null)
            {
                Debug.Log($"[InferenceSuggestionPanel] ② Port {port.name} has null ParentNode. Skip.");
                continue;
            }
            var seqNode = port.ParentNode;
            if (seqNode.TryGenerateSequentFromFrames(out var seq))
            {
                premises.Add(seq);
                Debug.Log($"[InferenceSuggestionPanel] ② Added premise sequent. Left={seq.Antecedents.Count}, Right={seq.Consequents.Count}");
            }
            else
            {
                Debug.Log($"[InferenceSuggestionPanel] ② TryGenerateSequentFromFrames failed on port {port.name}.");
            }
        }

        if (premises.Count == 0)
        {
            Debug.Log("[InferenceSuggestionPanel] No premise sequents found. Skip ②.");
            return;
        }

        // ルールノード: InferenceBar に紐づく RulePatch 上の CurrentRuleNode から Rule を取得
        var patchField = typeof(InferenceBar).GetField("rulePatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var patch = patchField != null ? patchField.GetValue(bar) as RulePatch : null;
        if (patch == null || patch.CurrentRuleNode == null)
        {
            Debug.Log("[InferenceSuggestionPanel] No RuleNode on RulePatch. Skip ②.");
            return;
        }

        var rule = patch.CurrentRuleNode.Rule;
        Debug.Log($"[InferenceSuggestionPanel] ② Using rule {rule} with {premises.Count} premises.");
        var conclusions = LKInference.EnumerateConclusions(premises, rule) ?? Enumerable.Empty<Sequent>();
        
        var conclusionList = conclusions.ToList();
        Debug.Log($"[InferenceSuggestionPanel] ② EnumerateConclusions returned {conclusionList.Count} conclusion candidates.");

        int created = 0;
        foreach (var seq in conclusionList)
        {
            Debug.Log($"[InferenceSuggestionPanel] ② Creating SequentNode for conclusion. Left={seq.Antecedents.Count}, Right={seq.Consequents.Count}");
            var node = NodeCreator.CreateSequentNode(seq, sequentNodePrefab, content);
            if (node == null)
            {
                Debug.LogWarning("[InferenceSuggestionPanel] ② NodeCreator.CreateSequentNode returned null for conclusion.");
                continue;
            }
            Debug.Log("[InferenceSuggestionPanel] ② SequentNode created successfully.");
            PreparePreviewSequentNode(node);
            created++;
        }

        Debug.Log($"[InferenceSuggestionPanel] ② Total SequentNodes created: {created}");
        AdjustContentHeight(created);
    }

    /// <summary>
    /// ③ 前提 + 結論シーケント から適用可能な Rule を列挙し、RuleNode を生成して content に追加する。
    /// </summary>
    private void TryShowRulesFromPremisesAndConclusion(InferenceBar bar)
    {
        // 前提シーケント収集（②と同様）
        var premises = new List<Sequent>();
        Debug.Log($"[InferenceSuggestionPanel] ③ Collect premises from {bar.PremisesPorts.Count} premise ports.");
        foreach (var port in bar.PremisesPorts)
        {
            if (port == null)
            {
                Debug.Log("[InferenceSuggestionPanel] ③ Found null port in PremisesPorts. Skip.");
                continue;
            }
            if (port.ParentNode == null)
            {
                Debug.Log($"[InferenceSuggestionPanel] ③ Port {port.name} has null ParentNode. Skip.");
                continue;
            }
            var seqNode = port.ParentNode;
            if (seqNode.TryGenerateSequentFromFrames(out var seq))
            {
                premises.Add(seq);
                Debug.Log($"[InferenceSuggestionPanel] ③ Added premise sequent. Left={seq.Antecedents.Count}, Right={seq.Consequents.Count}");
            }
            else
            {
                Debug.Log($"[InferenceSuggestionPanel] ③ TryGenerateSequentFromFrames failed on port {port.name}.");
            }
        }

        if (premises.Count == 0)
        {
            Debug.Log("[InferenceSuggestionPanel] No premise sequents found. Skip ③.");
            return;
        }

        // 結論シーケント: ConclusionPort の ParentNode から取得
        if (bar.ConclusionPort == null)
        {
            Debug.Log("[InferenceSuggestionPanel] ③ ConclusionPort is null. Skip ③.");
            return;
        }

        if (bar.ConclusionPort.ParentNode == null)
        {
            Debug.Log("[InferenceSuggestionPanel] ③ ConclusionPort.ParentNode is null. Skip ③.");
            return;
        }

        if (!bar.ConclusionPort.ParentNode.TryGenerateSequentFromFrames(out var conclusion))
        {
            Debug.Log("[InferenceSuggestionPanel] ③ Conclusion SequentNode not ready. Skip ③.");
            return;
        }

        var rules = LKInference.EnumerateRules(premises, conclusion) ?? Enumerable.Empty<Rule>();
        Debug.Log($"[InferenceSuggestionPanel] ③ EnumerateRules with {premises.Count} premises and conclusion Left={conclusion.Antecedents.Count}, Right={conclusion.Consequents.Count}.");
        int created = 0;
        foreach (var r in rules)
        {
            // NodeCreator 側のルールプレハブ対応表を使って Rule ごとの見た目を生成
            var rn = CreateRuleNodeForSuggestion(r);
            if (rn == null) continue;
            var rt = rn.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetParent(content, false);
                rt.localScale = Vector3.one;
                rt.anchoredPosition3D = Vector3.zero;
            }
            created++;
        }

        AdjustContentHeight(created);
    }

    // NodeCreator の RuleNodes プレハブ配列を使って、指定ルールの RuleNode を生成するヘルパ
    private RuleNode CreateRuleNodeForSuggestion(Rule rule)
    {
        // NodeCreator はシーンに1つある前提なので、そのインスタンスを探す
        var creator = Object.FindFirstObjectByType<NodeCreator>();
        if (creator == null)
        {
            Debug.LogError("[InferenceSuggestionPanel] NodeCreator not found in scene. Skip rule suggestions.");
            return null;
        }

        // NodeCreator の静的ヘルパを経由してプレハブ生成（parent は一旦 CanvasRect.Main、あとで content に付け替え）
        var rn = (RuleNode)creator
            .GetType()
            .GetMethod("CreateRuleNodeInstance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.Invoke(null, new object[] { rule, CanvasRect.Main });

        if (rn == null)
        {
            Debug.LogWarning($"[InferenceSuggestionPanel] Failed to create RuleNode for rule {rule}.");
        }
        return rn;
    }

    private void PreparePreviewSequentNode(SequentNode node)
    {
        var rt = node.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }
    }

    private void AdjustContentHeight(int createdCount)
    {
        if (content == null || createdCount <= 0) return;

        var layout = content.GetComponent<LayoutGroup>();
        if (layout == null)
        {
            float h = createdCount * 40f;
            content.sizeDelta = new Vector2(content.sizeDelta.x, h);
        }
    }

    /// <summary>
    /// 与えられた RectTransform がこの InferenceSuggestionPanel の content 内にあるかチェックする。
    /// </summary>
    public bool IsNodeInContent(RectTransform nodeRect)
    {
        if (content == null || nodeRect == null) return false;
        
        // nodeRect の親をたどって content に到達するかチェック
        var current = nodeRect.parent as RectTransform;
        while (current != null)
        {
            if (current == content) return true;
            current = current.parent as RectTransform;
        }
        return false;
    }

    /// <summary>
    /// 現在のスクロールビューコンテンツを全削除する。
    /// </summary>
    public void Clear()
    {
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
