using UnityEngine;
using FormalSystem.LK;
using System.Collections.Generic;

public class NodeCreator : MonoBehaviour
{

    [SerializeField] GameObject[] Variables;
    [SerializeField] GameObject[] Operators;
    [SerializeField] GameObject SequentPrefab;

    // 静的参照: 自動生成用にランタイムで登録
    static GameObject[] sVariables;
    static GameObject[] sOperators;
    void Awake()
    {
        if (sVariables == null || sVariables.Length == 0) sVariables = Variables;
        if (sOperators == null || sOperators.Length == 0) sOperators = Operators;
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

    public void CreateSequent()
    {
        Instantiate(SequentPrefab, CanvasRect.Main);
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
        var inst = Object.Instantiate(sequentPrefab, parent);

        // Antecedents
        for (int i = 0; i < sequent.Antecedents.Count; i++)
        {
            inst.AddLeftFrame();
            var frame = inst.LeftFrames[inst.LeftFrames.Count - 1];
            var node = CreateNodeFromFormula(sequent.Antecedents[i], frame.RectTransform);
            frame.SetNodeDirect(node);
        }
        // Consequents
        for (int i = 0; i < sequent.Consequents.Count; i++)
        {
            inst.AddRightFrame();
            var frame = inst.RightFrames[inst.RightFrames.Count - 1];
            var node = CreateNodeFromFormula(sequent.Consequents[i], frame.RectTransform);
            frame.SetNodeDirect(node);
        }
        // 内部 Sequent プロパティ更新（TryGenerateSequent が自動で拾う想定だが明示的にも）
        inst.TryGenerateSequentFromFrames(out _);
        return inst;
    }
}
