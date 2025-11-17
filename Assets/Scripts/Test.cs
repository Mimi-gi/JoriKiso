using UnityEngine;
using System.Collections.Generic;
using FormalSystem.LK;

public class Test : MonoBehaviour
{
    ProofTree tree;
    void Start()
    {
        TestImpRInference();
    }
    public void On()
    {
        Debug.Log(tree.IsValid());
    }

    public void TestImpRInference()
    {
        // テスト: ¬B, A → B ⊢ ¬A から →R で推論可能な結論を列挙
        var B = new Variable(Alphabet.B);
        var A = new Variable(Alphabet.A);
        var notB = new Not(B);
        var notA = new Not(A);
        var impAB = new Implication(A, B);

        // 前提シーケント: ¬B, A → B ⊢ ¬A
        var premise = new Sequent(
            new Formulas(new Formula[] { notB, impAB }),
            new Formulas(new Formula[] { notA })
        );

        Debug.Log("[Test.TestImpRInference] 前提シーケント: ¬B, A → B ⊢ ¬A");
        Debug.Log($"  前件: {string.Join(", ", premise.Antecedents)}");
        Debug.Log($"  後件: {string.Join(", ", premise.Consequents)}");

        // →R で推論可能な結論を列挙 (Distinctなしで)
        var conclusions = LKInference.EnumerateConclusions(new[] { premise }, Rule.impR);

        Debug.Log($"[Test.TestImpRInference] →R で推論可能な結論 (Distinctなし):");
        int count = 0;
        foreach (var conclusion in conclusions)
        {
            count++;
            var anteStr = conclusion.Antecedents.Count > 0 
                ? string.Join(", ", conclusion.Antecedents) 
                : "(empty)";
            var consStr = conclusion.Consequents.Count > 0 
                ? string.Join(", ", conclusion.Consequents) 
                : "(empty)";
            Debug.Log($"  [{count}] {anteStr} ⊢ {consStr}");
        }

        if (count == 0)
        {
            Debug.Log("  (結論なし)");
        }
    }
}
