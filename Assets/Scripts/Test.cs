using UnityEngine;
using System.Collections.Generic;
using FormalSystem.LK;

public class Test : MonoBehaviour
{
    ProofTree tree;
    void Start()
    {
        // 簡単な証明木の構築テスト
        // 1) 公理 I: A ⊢ A（葉）
        var A = new Variable(Alphabet.A);
        var axiomSequent = new Sequent(
            new Formulas(new Formula[] { A }),
            new Formulas(new Formula[] { A })
        );
        var axiom = new Proof(new List<Proof>(), axiomSequent, Rule.I);

        // 2) notR: 前提 Γ, A ⊢ Δ から 結論 Γ ⊢ Δ, ¬A
        //    ここでは Γ=∅, Δ={A} のケースとして、I を前提に用いる
        var notRConclusion = new Sequent(
            new Formulas(new Formula[] { }),
            new Formulas(new Formula[] { A, new Not(A) })
        );
        var notRProof = new Proof(new List<Proof> { axiom }, notRConclusion, Rule.notR);

        // 証明木: 根が notR、子に I
        tree = new ProofTree(notRProof);
    }
    public void On()
    {
        Debug.Log(tree.IsValid());
    }
}
