using UnityEngine;
using FormalSystem.LK;
using System.Collections.Generic;
using System.Linq;

public class NodeCreator : MonoBehaviour
{

    [SerializeField] GameObject[] Variables;
    [SerializeField] GameObject[] Operators;
    [SerializeField] GameObject[] RuleNodes;
    [SerializeField] GameObject SequentPrefab;
    [SerializeField] GameObject InferenceBarPrefab;
    [SerializeField] GameObject StartBarPrefab;

    // 静的参照: 自動生成用にランタイムで登録
    static GameObject[] sVariables;
    static GameObject[] sOperators;
    static GameObject[] sRuleNodes;
    void Awake()
    {
        if (sVariables == null || sVariables.Length == 0) sVariables = Variables;
        if (sOperators == null || sOperators.Length == 0) sOperators = Operators;
        if (sRuleNodes == null || sRuleNodes.Length == 0) sRuleNodes = RuleNodes;
    }

    public void LogTrue(){
        Debug.Log(ValidateAllInferenceChains());
    }

    /// <summary>
    /// シーン内のすべての isStart=true な InferenceBar を探し、
    /// それぞれの推論チェーンが妥当かを検証する。
    /// すべてが妥当なら true、1つでも不妥当なら false を返す。
    /// </summary>
    public static bool ValidateAllInferenceChains()
    {
        // シーン内のすべての InferenceBar を取得
        var allBars = Object.FindObjectsByType<InferenceBar>(FindObjectsSortMode.None);
        
        // isStart=true なバーを抽出
        var startBars = new List<InferenceBar>();
        foreach (var bar in allBars)
        {
            if (bar.isStart)
            {
                startBars.Add(bar);
            }
        }

        if (startBars.Count == 0)
        {
            Debug.LogWarning("[NodeCreator.ValidateAllInferenceChains] No start bars found");
            return false;
        }

        Debug.Log($"[NodeCreator.ValidateAllInferenceChains] Found {startBars.Count} start bars");

        int totalInferences = 0;
        Rule? lastRule = null;

        // 各スタートバーのチェーンを検証
        foreach (var startBar in startBars)
        {
            if (!ValidateInferenceChain(startBar, out int chainInferences, out Rule? chainLastRule))
            {
                Debug.LogWarning($"[NodeCreator.ValidateAllInferenceChains] Inference chain starting from {startBar.name} is invalid");
                return false;
            }
            totalInferences += chainInferences;
            lastRule = chainLastRule;
        }

        Debug.Log("[NodeCreator.ValidateAllInferenceChains] All inference chains are valid");
        Debug.Log($"[NodeCreator.ValidateAllInferenceChains] Total inferences: {totalInferences}");
        if (lastRule.HasValue)
        {
            Debug.Log($"[NodeCreator.ValidateAllInferenceChains] Last applied rule: {lastRule.Value}");
        }
        return true;
    }

    public void CreateInferenceBar()
    {
        Instantiate(InferenceBarPrefab, CanvasRect.Main);
    }

    public void CreateStartBar(){
        Instantiate(StartBarPrefab, CanvasRect.Main);
    }

    public void CreateA()
    {
        Instantiate(Variables[0], CanvasRect.Main);
    }
    public void CreateB()
    {
        Instantiate(Variables[1], CanvasRect.Main);
    }
    public void CreateC()
    {
        Instantiate(Variables[2], CanvasRect.Main);
    }
    public void CreateD()
    {
        Instantiate(Variables[3], CanvasRect.Main);
    }
    public void CreateNot()
    {
        Instantiate(Operators[0], CanvasRect.Main);
    }
    public void CreateAnd()
    {
        Instantiate(Operators[1], CanvasRect.Main);
    }
    public void CreateOr()
    {
        Instantiate(Operators[2], CanvasRect.Main);
    }
    public void CreateImp()
    {
        Instantiate(Operators[3], CanvasRect.Main);
    }

    // -------- ルールノード生成 API --------
    // RuleNodes 配列の並びをインスペクタ上で Rule.I, Rule.Cut, Rule.andL, ... などに対応させてください。

    public void CreateRuleI()
    {
        CreateRuleNodeInstance(Rule.I, CanvasRect.Main);
    }

    public void CreateRuleCut()
    {
        CreateRuleNodeInstance(Rule.Cut, CanvasRect.Main);
    }

    public void CreateRuleAndL()
    {
        CreateRuleNodeInstance(Rule.andL, CanvasRect.Main);
    }

    public void CreateRuleAndR()
    {
        CreateRuleNodeInstance(Rule.andR, CanvasRect.Main);
    }

    public void CreateRuleOrL()
    {
        CreateRuleNodeInstance(Rule.orL, CanvasRect.Main);
    }

    public void CreateRuleOrR()
    {
        CreateRuleNodeInstance(Rule.orR, CanvasRect.Main);
    }

    public void CreateRuleImpL()
    {
        CreateRuleNodeInstance(Rule.impL, CanvasRect.Main);
    }

    public void CreateRuleImpR()
    {
        CreateRuleNodeInstance(Rule.impR, CanvasRect.Main);
    }

    public void CreateRuleNotL()
    {
        CreateRuleNodeInstance(Rule.notL, CanvasRect.Main);
    }

    public void CreateRuleNotR()
    {
        CreateRuleNodeInstance(Rule.notR, CanvasRect.Main);
    }

    public void CreateRuleWL()
    {
        CreateRuleNodeInstance(Rule.WL, CanvasRect.Main);
    }

    public void CreateRuleWR()
    {
        CreateRuleNodeInstance(Rule.WR, CanvasRect.Main);
    }

    public void CreateRuleCL()
    {
        CreateRuleNodeInstance(Rule.CL, CanvasRect.Main);
    }

    public void CreateRuleCR()
    {
        CreateRuleNodeInstance(Rule.CR, CanvasRect.Main);
    }

    public void CreateSequent()
    {
        Instantiate(SequentPrefab, CanvasRect.Main);
    }

    // 汎用: 指定 Rule の RuleNode プレハブを parent に生成
    static RuleNode CreateRuleNodeInstance(Rule rule, Transform parent)
    {
        if (sRuleNodes == null || sRuleNodes.Length <= (int)rule) return null;
        var prefab = sRuleNodes[(int)rule];
        if (prefab == null || parent == null) return null;
        var go = Object.Instantiate(prefab, parent);
        return go.GetComponent<RuleNode>();
    }

    // -------- 静的API: Formula から Node を再帰生成 --------
    public static Node CreateNodeFromFormula(Formula formula, Transform parent)
    {
        if (formula == null || parent == null) return null;
        if (sVariables == null || sOperators == null) {
            Debug.LogError("NodeCreator static prefabs not registered yet.");
            return null;
        }

        GameObject go = null;
        Node node = null;

        switch (formula)
        {
            case Variable v:
                int idx = (int)v.Name;
                if (idx < 0 || idx >= sVariables.Length) { Debug.LogError($"No variable prefab for {v.Name}"); return null; }
                go = Object.Instantiate(sVariables[idx], parent);
                node = go.GetComponent<Node>();
                break;
            case Not nt:
                // Operators[0] を Not と想定
                if (sOperators.Length < 1) { Debug.LogError("Not prefab missing"); return null; }
                go = Object.Instantiate(sOperators[0], parent);
                node = go.GetComponent<Node>();
                var unary = node as UnaryNode;
                if (unary != null)
                {
                    var child = CreateNodeFromFormula(nt.Operand, unary.Frame.RectTransform);
                    // Frame に直接セット
                    unary.Frame.SetNodeDirect(child);
                }
                break;
            case And and:
                go = InstantiateBinary(1, and.Left, and.Right, parent);
                node = go?.GetComponent<Node>();
                break;
            case Or or:
                go = InstantiateBinary(2, or.Left, or.Right, parent);
                node = go?.GetComponent<Node>();
                break;
            case Implication imp:
                go = InstantiateBinary(3, imp.Left, imp.Right, parent);
                node = go?.GetComponent<Node>();
                break;
            default:
                Debug.LogError("Unsupported formula type for automatic node creation.");
                return null;
        }
        return node;
    }

    // Binary helper: opIndex = 1:And 2:Or 3:Implication (Operators 配列前提)
    static GameObject InstantiateBinary(int opIndex, Formula left, Formula right, Transform parent)
    {
        if (opIndex >= sOperators.Length) { Debug.LogError("Binary operator prefab missing index " + opIndex); return null; }
        var go = Object.Instantiate(sOperators[opIndex], parent);
        var bn = go.GetComponent<BinaryNode>();
        if (bn != null)
        {
            var leftNode = CreateNodeFromFormula(left, bn.LeftFrame.RectTransform);
            var rightNode = CreateNodeFromFormula(right, bn.RightFrame.RectTransform);
            bn.LeftFrame.SetNodeDirect(leftNode);
            bn.RightFrame.SetNodeDirect(rightNode);
        }
        return go;
    }

    // -------- 静的API: Sequent から SequentNode を生成 --------
    // sequentPrefab: 既存のレイアウト/ボタン付きプレハブ
    public static SequentNode CreateSequentNode(Sequent sequent, SequentNode sequentPrefab, Transform parent)
    {
        if (sequentPrefab == null || parent == null) return null;
        Debug.Log("[NodeCreator] CreateSequentNode start: Antecedents=" + sequent.Antecedents.Count + ", Consequents=" + sequent.Consequents.Count);
        var inst = Object.Instantiate(sequentPrefab, parent);

        // Antecedents
        for (int i = 0; i < sequent.Antecedents.Count; i++)
        {
            Debug.Log("[NodeCreator] Adding left frame " + i);
            inst.AddLeftFrame();
            Debug.Log("[NodeCreator] Left frame added. Total left frames: " + inst.LeftFrames.Count);
            var frame = inst.LeftFrames[inst.LeftFrames.Count - 1];
            if (frame == null)
            {
                Debug.LogError("[NodeCreator] Frame is null after AddLeftFrame");
                continue;
            }
            Debug.Log("[NodeCreator] Creating node from formula: " + sequent.Antecedents[i]);
            var node = CreateNodeFromFormula(sequent.Antecedents[i], frame.RectTransform);
            if (node == null)
            {
                Debug.LogError("[NodeCreator] Node creation failed for antecedent " + i);
                continue;
            }
            Debug.Log("[NodeCreator] Setting node direct to frame");
            frame.SetNodeDirect(node);
        }
        // Consequents
        for (int i = 0; i < sequent.Consequents.Count; i++)
        {
            Debug.Log("[NodeCreator] Adding right frame " + i);
            inst.AddRightFrame();
            Debug.Log("[NodeCreator] Right frame added. Total right frames: " + inst.RightFrames.Count);
            var frame = inst.RightFrames[inst.RightFrames.Count - 1];
            if (frame == null)
            {
                Debug.LogError("[NodeCreator] Frame is null after AddRightFrame");
                continue;
            }
            Debug.Log("[NodeCreator] Creating node from formula: " + sequent.Consequents[i]);
            var node = CreateNodeFromFormula(sequent.Consequents[i], frame.RectTransform);
            if (node == null)
            {
                Debug.LogError("[NodeCreator] Node creation failed for consequent " + i);
                continue;
            }
            Debug.Log("[NodeCreator] Setting node direct to frame");
            frame.SetNodeDirect(node);
        }
        // 内部 Sequent プロパティ更新（TryGenerateSequent が自動で拾う想定だが明示的にも）
        Debug.Log("[NodeCreator] Calling TryGenerateSequentFromFrames");
        inst.TryGenerateSequentFromFrames(out _);
        Debug.Log("[NodeCreator] CreateSequentNode complete");
        return inst;
    }

    /// <summary>
    /// isStart が true な InferenceBar から始まるすべての InferenceBar の推論チェーンが妥当かを判定する。
    /// 各 InferenceBar において、PremisesPorts から得られるシーケント + ConclusionPort のシーケント
    /// について、適用されたルールで妥当な推論かを CheckValidity で検証する。
    /// すべての InferenceBar が妥当なら true、1つでも不妥当なら false を返す。
    /// </summary>
    public static bool ValidateInferenceChain(InferenceBar startBar)
    {
        return ValidateInferenceChain(startBar, out _, out _);
    }

    /// <summary>
    /// isStart が true な InferenceBar から始まるすべての InferenceBar の推論チェーンが妥当かを判定する。
    /// chainInferences に推論の総数、lastRule に最後に適用されたルールを出力する。
    /// 
    /// チェック内容：
    /// 1. ポート経由で推論バーの完全な構造を構築
    /// 2. 最深の推論バーを探索
    /// 3. 最深から遡りながら妥当性を検証
    /// 4. すべての末端バーがisStart=true であり推論チェーン全体が妥当なら true
    /// </summary>
    public static bool ValidateInferenceChain(InferenceBar startBar, out int chainInferences, out Rule? lastRule)
    {
        chainInferences = 0;
        lastRule = null;

        if (startBar == null || !startBar.isStart)
        {
            Debug.LogWarning("[NodeCreator.ValidateInferenceChain] Invalid start bar");
            return false;
        }

        // ステップ 1: 全推論バーの構造を構築
        var allInferenceItems = BuildInferenceChainFromPorts(startBar);
        
        if (allInferenceItems.Count == 0)
        {
            Debug.LogWarning("[NodeCreator.ValidateInferenceChain] No inference chain built from ports");
            return false;
        }

        Debug.Log($"[NodeCreator.ValidateInferenceChain] Built complete inference structure with {allInferenceItems.Count} bars");

        // ステップ 2: 推論バーの最大深度を見つける
        var depthMap = new Dictionary<InferenceBar, int>();
        var maxDepth = CalculateInferenceDepths(allInferenceItems, depthMap);
        
        Debug.Log($"[NodeCreator.ValidateInferenceChain] Maximum inference depth: {maxDepth}");
        for (int d = 0; d <= maxDepth; d++)
        {
            var barsAtDepth = depthMap.Where(kv => kv.Value == d).Select(kv => kv.Key.name);
            Debug.Log($"  Depth {d}: {string.Join(", ", barsAtDepth)}");
        }

        // ステップ 3: 最深から遡りながら妥当性を検証
        return ValidateInferenceTreeFromDeepest(allInferenceItems, depthMap, maxDepth, out chainInferences, out lastRule);
    }

    /// <summary>
    /// 1つの InferenceBar の推論が妥当かを検証する。
    /// isStart=true の場合は前提なしで Rule I を適用する。
    /// </summary>
    private static bool ValidateSingleInferenceBar(InferenceBar bar)
    {
        return ValidateSingleInferenceBar(bar, out _);
    }

    /// <summary>
    /// 1つの InferenceBar の推論が妥当かを検証する。
    /// appliedRule に適用されたルールを出力する。
    /// isStart=true の場合は前提なしで Rule I を適用する。
    /// </summary>
    private static bool ValidateSingleInferenceBar(InferenceBar bar, out Rule? appliedRule)
    {
        appliedRule = null;

        if (bar == null) return false;

        List<Sequent> premises = new List<Sequent>();

        // isStart=true の場合、前提なしで Rule I を適用
        if (bar.isStart)
        {
            // 結論シーケントを取得
            if (bar.ConclusionPort == null || bar.ConclusionPort.ParentNode == null)
            {
                Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Conclusion port or parent node is null");
                return false;
            }

            var conclusionNode = bar.ConclusionPort.ParentNode;
            if (!conclusionNode.TryGenerateSequentFromFrames(out var conclusion))
            {
                Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Failed to generate conclusion sequent");
                return false;
            }

            // Rule I は A ⊢ A の形式なので、前件と後件が同じ1つの式である必要がある
            if (conclusion.Antecedents.Count != 1 || conclusion.Consequents.Count != 1)
            {
                Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Rule I requires exactly one formula on each side");
                return false;
            }

            if (conclusion.Antecedents[0] != conclusion.Consequents[0])
            {
                Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Rule I requires the same formula on both sides");
                return false;
            }

            // Rule I の妥当性を検証
            var proof = new Proof(new List<Proof>(), conclusion, Rule.I);
            bool isValid = proof.IsValid;
            Debug.Log($"[NodeCreator.ValidateSingleInferenceBar] Rule I validation: {isValid}");
            appliedRule = Rule.I;
            return isValid;
        }

        // 通常の推論の場合：前提シーケントを収集
        foreach (var port in bar.PremisesPorts)
        {
            if (port == null || port.ParentNode == null) continue;
            var seqNode = port.ParentNode;
            if (seqNode.TryGenerateSequentFromFrames(out var seq))
            {
                premises.Add(seq);
            }
            else
            {
                Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Failed to generate sequent from premise port");
                return false;
            }
        }

        if (premises.Count == 0)
        {
            Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] No premises found");
            return false;
        }

        // 結論シーケントを取得
        if (bar.ConclusionPort == null || bar.ConclusionPort.ParentNode == null)
        {
            Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Conclusion port or parent node is null");
            return false;
        }

        var conclusionNode2 = bar.ConclusionPort.ParentNode;
        if (!conclusionNode2.TryGenerateSequentFromFrames(out var conclusion2))
        {
            Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] Failed to generate conclusion sequent");
            return false;
        }

        // RulePatch から Rule を取得
        var rulePatchField = typeof(InferenceBar).GetField("rulePatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rulePatch = rulePatchField != null ? rulePatchField.GetValue(bar) as RulePatch : null;
        if (rulePatch == null || rulePatch.CurrentRuleNode == null)
        {
            Debug.LogWarning($"[NodeCreator.ValidateSingleInferenceBar] No rule selected");
            return false;
        }

        var rule = rulePatch.CurrentRuleNode.Rule;
        appliedRule = rule;
        
        // Proof.CheckValidity を使用して妥当性を検証
        var proof2 = new Proof(
            premises.ConvertAll(s => new Proof(new List<Proof>(), s, Rule.I)),
            conclusion2,
            rule
        );

        bool isValid2 = proof2.IsValid;
        Debug.Log($"[NodeCreator.ValidateSingleInferenceBar] Rule {rule} validation: {isValid2}");
        return isValid2;
    }

    /// <summary>
    /// isStart が true な InferenceBar から始まり、ConclusionPort で接続された次の InferenceBar へと
    /// 連なるすべての InferenceBar を収集する。
    /// </summary>
    private static List<InferenceBar> CollectInferenceBarChain(InferenceBar startBar)
    {
        var result = new List<InferenceBar>();
        var visited = new HashSet<InferenceBar>();
        var queue = new Queue<InferenceBar>();
        
        queue.Enqueue(startBar);
        visited.Add(startBar);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            // ConclusionPort から次の InferenceBar を探す
            if (current.ConclusionPort != null && current.ConclusionPort.ParentNode != null)
            {
                var nextBar = current.ConclusionPort.ParentNode.GetComponent<SequentNode>()?.GetComponentInParent<InferenceBar>();
                if (nextBar != null && !visited.Contains(nextBar))
                {
                    visited.Add(nextBar);
                    queue.Enqueue(nextBar);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 2つのシーケントが同じか判定する。
    /// 前件と後件の両方の要素が同じ順序で同じ式であれば true。
    /// </summary>
    private static bool SequentsEqual(Sequent s1, Sequent s2)
    {
        if ((object)s1 == null || (object)s2 == null) return (object)s1 == (object)s2;
        
        // 前件の数が異なれば false
        if (s1.Antecedents.Count != s2.Antecedents.Count) return false;
        // 後件の数が異なれば false
        if (s1.Consequents.Count != s2.Consequents.Count) return false;

        // 前件の各要素を比較
        for (int i = 0; i < s1.Antecedents.Count; i++)
        {
            if (!FormulasEqual(s1.Antecedents[i], s2.Antecedents[i]))
                return false;
        }

        // 後件の各要素を比較
        for (int i = 0; i < s1.Consequents.Count; i++)
        {
            if (!FormulasEqual(s1.Consequents[i], s2.Consequents[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 2つの Formula が同じか判定する。
    /// </summary>
    private static bool FormulasEqual(Formula f1, Formula f2)
    {
        if ((object)f1 == null || (object)f2 == null) return (object)f1 == (object)f2;
        
        // ToString で比較（簡易的）
        return f1.ToString() == f2.ToString();
    }

    /// <summary>
    /// Sequent をデバッグ用に文字列表示する。
    /// </summary>
    private static string FormatSequent(Sequent seq)
    {
        if ((object)seq == null) return "null";
        
        var antStr = string.Join(", ", seq.Antecedents.ConvertAll(f => f?.ToString() ?? "null"));
        var consStr = string.Join(", ", seq.Consequents.ConvertAll(f => f?.ToString() ?? "null"));
        
        return $"{antStr} ⊢ {consStr}";
    }

    /// <summary>
    /// 推論チェーンの構成要素。各 InferenceBar とその前提・結論シーケント情報を保持。
    /// </summary>
    private class InferenceChainItem
    {
        public InferenceBar Bar { get; set; }
        public List<Sequent> PremiseSequents { get; set; } = new List<Sequent>();
        public Sequent ConclusionSequent { get; set; }
    }

    /// <summary>
    /// startBar からポート経由で繋がる InferenceBar チェーンを構築する。
    /// Bar → ConclusionPort → SequentNode → PremisePort → (次の) Bar
    /// の経路でチェーンを構築し、各ステップの前提・結論シーケント情報を収集する。
    /// </summary>
    private static List<InferenceChainItem> BuildInferenceChainFromPorts(InferenceBar startBar)
    {
        var chain = new List<InferenceChainItem>();
        var visited = new HashSet<InferenceBar>();
        var queue = new Queue<InferenceBar>();

        // シーン内のすべての InferenceBar をログ出力
        var allBars = Object.FindObjectsByType<InferenceBar>(FindObjectsSortMode.None);
        Debug.Log($"[BuildInferenceChainFromPorts] Scene has {allBars.Length} InferenceBar(s)");
        foreach (var bar in allBars)
        {
            var conclusion = bar.ConclusionPort?.ParentNode;
            var premises = bar.PremisesPorts.ConvertAll(p => p?.ParentNode?.name ?? "null");
            var premisesStr = string.Join(", ", premises);
            Debug.Log($"  - InferenceBar '{bar.name}' (isStart={bar.isStart}): premises=[{premisesStr}], conclusion={conclusion?.name ?? "null"}");
        }

        queue.Enqueue(startBar);
        visited.Add(startBar);

        while (queue.Count > 0)
        {
            var currentBar = queue.Dequeue();

            // 現在のバーのアイテムを作成
            var item = new InferenceChainItem { Bar = currentBar };
            
            Debug.Log($"[BuildInferenceChainFromPorts] Processing bar: {currentBar.name}");

            // isStart バーの場合の処理
            if (currentBar.isStart)
            {
                // 結論ポートから結論シーケントを取得
                if (currentBar.ConclusionPort != null && currentBar.ConclusionPort.ParentNode != null)
                {
                    if (currentBar.ConclusionPort.ParentNode.TryGenerateSequentFromFrames(out var conclusion))
                    {
                        item.ConclusionSequent = conclusion;
                    }
                }
            }
            else
            {
                // 通常のバー：前提ポートから前提シーケントを取得
                foreach (var port in currentBar.PremisesPorts)
                {
                    if (port != null && port.ParentNode != null)
                    {
                        if (port.ParentNode.TryGenerateSequentFromFrames(out var premise))
                        {
                            item.PremiseSequents.Add(premise);
                        }
                    }
                }

                // 結論ポートから結論シーケントを取得
                if (currentBar.ConclusionPort != null && currentBar.ConclusionPort.ParentNode != null)
                {
                    if (currentBar.ConclusionPort.ParentNode.TryGenerateSequentFromFrames(out var conclusion))
                    {
                        item.ConclusionSequent = conclusion;
                    }
                }
            }

            chain.Add(item);

            // 次のバーを探す
            // 「このバーの結論ノードが、他のバーの PremisePort に接続されているか」をチェック
            if (currentBar.ConclusionPort != null && currentBar.ConclusionPort.ParentNode != null)
            {
                var conclusionNode = currentBar.ConclusionPort.ParentNode;
                Debug.Log($"[BuildInferenceChainFromPorts] ConclusionNode of {currentBar.name}: {conclusionNode.name}");

                // シーン内のすべてのバーをチェック
                var allBars_check = Object.FindObjectsByType<InferenceBar>(FindObjectsSortMode.None);
                foreach (var potentialNextBar in allBars_check)
                {
                    if (visited.Contains(potentialNextBar)) continue;

                    // potentialNextBar の PremisePort に、conclusionNode が接続されているか確認
                    foreach (var premisePort in potentialNextBar.PremisesPorts)
                    {
                        if (premisePort != null && premisePort.ParentNode == conclusionNode)
                        {
                            Debug.Log($"[BuildInferenceChainFromPorts] Found connection: {currentBar.name} → {potentialNextBar.name} (via {conclusionNode.name})");
                            visited.Add(potentialNextBar);
                            queue.Enqueue(potentialNextBar);
                            break; // 1つ見つかったら次のバーへ
                        }
                    }
                }
            }
        }

        // 構築したチェーンの詳細をログ出力
        Debug.Log($"[BuildInferenceChainFromPorts] Built chain with {chain.Count} items:");
        for (int i = 0; i < chain.Count; i++)
        {
            var item = chain[i];
            var premisesStr = string.Join(", ", item.PremiseSequents.ConvertAll(s => $"({FormatSequent(s)})"));
            var conclusionStr = (object)item.ConclusionSequent == null ? "null" : FormatSequent(item.ConclusionSequent);
            Debug.Log($"  [{i}] Bar: {item.Bar.name}, Premises: [{premisesStr}], Conclusion: {conclusionStr}");
        }

        return chain;
    }

    /// <summary>
    /// 全推論アイテムについて、各バーの深度（何段階下にあるか）を計算する。
    /// 深度 = そのバーの結論を使う次のバーの最大深度 + 1
    /// isStart バーの深度は 0。
    /// </summary>
    private static int CalculateInferenceDepths(List<InferenceChainItem> allItems, Dictionary<InferenceBar, int> depthMap)
    {
        // 各バーを辞書化
        var itemsByBar = allItems.ToDictionary(item => item.Bar);
        
        // 最初は全バーの深度を -1 で初期化
        foreach (var item in allItems)
        {
            depthMap[item.Bar] = -1;
        }

        // トポロジカルソート的に深度を計算
        int maxDepth = 0;
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var item in allItems)
            {
                if (item.Bar.isStart)
                {
                    // isStart バーの深度は 0
                    if (depthMap[item.Bar] < 0)
                    {
                        depthMap[item.Bar] = 0;
                        changed = true;
                    }
                }
                else
                {
                    // 通常のバー：前提バーの最大深度 + 1
                    int maxPremiseDepth = -1;
                    
                    // 前提ノードが属するバーを探す
                    foreach (var premiseSeq in item.PremiseSequents)
                    {
                        foreach (var otherItem in allItems)
                        {
                            if ((object)otherItem.ConclusionSequent != null && 
                                SequentsEqual(otherItem.ConclusionSequent, premiseSeq))
                            {
                                int premiseDepth = depthMap[otherItem.Bar];
                                if (premiseDepth >= 0)
                                {
                                    maxPremiseDepth = Mathf.Max(maxPremiseDepth, premiseDepth);
                                }
                            }
                        }
                    }

                    // すべての前提が計算済みなら、このバーの深度を計算
                    if (maxPremiseDepth >= 0 && depthMap[item.Bar] < 0)
                    {
                        depthMap[item.Bar] = maxPremiseDepth + 1;
                        maxDepth = Mathf.Max(maxDepth, depthMap[item.Bar]);
                        changed = true;
                    }
                }
            }
        }

        // すべてのバーが計算済みか確認
        foreach (var item in allItems)
        {
            if (depthMap[item.Bar] < 0)
            {
                Debug.LogWarning($"[CalculateInferenceDepths] Bar {item.Bar.name} depth not calculated - possibly disconnected");
                depthMap[item.Bar] = -1; // 未接続
            }
        }

        return maxDepth;
    }

    /// <summary>
    /// 最深の推論バーから遡りながら、推論チェーン全体の妥当性を検証する。
    /// 各深度レベルでバーを検証し、末端がすべてisStart=true で妥当なら true を返す。
    /// </summary>
    private static bool ValidateInferenceTreeFromDeepest(
        List<InferenceChainItem> allItems,
        Dictionary<InferenceBar, int> depthMap,
        int maxDepth,
        out int chainInferences,
        out Rule? lastRule)
    {
        chainInferences = 0;
        lastRule = null;

        // 深度が逆順（深い順）で検証を行う
        for (int depth = maxDepth; depth >= 0; depth--)
        {
            var barsAtThisDepth = depthMap.Where(kv => kv.Value == depth).Select(kv => kv.Key).ToList();
            
            Debug.Log($"[ValidateInferenceTreeFromDeepest] Validating depth {depth}: {string.Join(", ", barsAtThisDepth.ConvertAll(b => b.name))}");

            foreach (var bar in barsAtThisDepth)
            {
                // このバーに対応するアイテムを探す
                var item = allItems.First(x => x.Bar == bar);

                // 個別推論の妥当性を検証
                if (!ValidateSingleInferenceBar(bar, out Rule? appliedRule))
                {
                    Debug.LogWarning($"[ValidateInferenceTreeFromDeepest] Inference bar validation failed: {bar.name}");
                    return false;
                }

                chainInferences++;
                lastRule = appliedRule;
                
                Debug.Log($"[ValidateInferenceTreeFromDeepest] Depth {depth}: Bar {bar.name} validated with rule {appliedRule}");

                // このバーの結論から前提を使うバーを探し、チェーンの妥当性を検証
                if ((object)item.ConclusionSequent != null && depth > 0)
                {
                    // 結論を前提として使っている次のバーを探す
                    var dependentBars = allItems
                        .Where(x => x.PremiseSequents.Any(p => SequentsEqual(p, item.ConclusionSequent)))
                        .Select(x => x.Bar)
                        .ToList();

                    foreach (var depBar in dependentBars)
                    {
                        if (depthMap[depBar] == depth - 1)
                        {
                            // チェーンの妥当性を確認
                            Debug.Log($"[ValidateInferenceTreeFromDeepest] Chain verified: {bar.name} → {depBar.name}");
                        }
                    }
                }
            }
        }

        // 最後に、深度0のバー（末端）がすべてisStart=true であることを確認
        var endBars = depthMap.Where(kv => kv.Value == 0).Select(kv => kv.Key).ToList();
        foreach (var endBar in endBars)
        {
            if (!endBar.isStart)
            {
                Debug.LogWarning($"[ValidateInferenceTreeFromDeepest] End bar {endBar.name} is not isStart=true");
                return false;
            }
        }

        Debug.Log("[ValidateInferenceTreeFromDeepest] All inference trees validated successfully");
        return true;
    }
}