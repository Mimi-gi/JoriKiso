using System;
using System.Collections.Generic;
using System.Linq;
using static Alphabet;


public enum Alphabet
{
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K
}

namespace FormalSystem.LK
{
    public abstract class Formula
    {
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
        public static bool operator ==(Formula a, Formula b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Formula a, Formula b) => !(a == b);
    }

    public class Variable : Formula
    {
        public readonly Alphabet Name;
        public Variable(Alphabet name)
        {
            Name = name;
        }

        public static bool operator ==(Variable a, Variable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Name == b.Name;
        }

        public static bool operator !=(Variable a, Variable b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Variable other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class UnaryOperator : Formula
    {
        public readonly Formula Operand;
        public UnaryOperator(Formula operand)
        {
            Operand = operand;
        }

        public static bool operator ==(UnaryOperator a, UnaryOperator b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a.GetType() != b.GetType()) return false;
            return a.Operand == b.Operand;
        }

        public static bool operator !=(UnaryOperator a, UnaryOperator b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is UnaryOperator other && obj.GetType() == GetType())
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), Operand);
        }
    }

    public class Not : UnaryOperator
    {
        public Not(Formula operand) : base(operand) { }

        public static bool operator ==(Not a, Not b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Operand == b.Operand;
        }
        public static bool operator !=(Not a, Not b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Not other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Operand.GetHashCode();
        }
    }

    public class BinaryOperator : Formula
    {
        public readonly Formula Left;
        public readonly Formula Right;
        public BinaryOperator(Formula left, Formula right)
        {
            Left = left;
            Right = right;
        }

        public static bool operator ==(BinaryOperator a, BinaryOperator b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a.GetType() != b.GetType()) return false;
            return a.Left == b.Left && a.Right == b.Right;
        }

        public static bool operator !=(BinaryOperator a, BinaryOperator b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is BinaryOperator other && obj.GetType() == GetType())
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), Left, Right);
        }
    }
    public class And : BinaryOperator
    {
        public And(Formula left, Formula right) : base(left, right) { }
        public static bool operator ==(And a, And b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return (a.Left == b.Left && a.Right == b.Right) || (a.Left == b.Right && a.Right == b.Left);
        }
        public static bool operator !=(And a, And b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is And other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Right.GetHashCode();
        }
    }
    public class Or : BinaryOperator
    {
        public Or(Formula left, Formula right) : base(left, right) { }
        public static bool operator ==(Or a, Or b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return (a.Left == b.Left && a.Right == b.Right) || (a.Left == b.Right && a.Right == b.Left);
        }
        public static bool operator !=(Or a, Or b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Or other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Right.GetHashCode();
        }
    }
    public class Implication : BinaryOperator
    {
        public Implication(Formula left, Formula right) : base(left, right) { }
        public static bool operator ==(Implication a, Implication b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Left == b.Left && a.Right == b.Right;
        }
        public static bool operator !=(Implication a, Implication b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Implication other)
            {
                return this == other;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Right.GetHashCode();
        }
    }

    //3項、4項などももし実装するなら...

    public class Formulas : List<Formula>
    {
        public Formulas(IEnumerable<Formula> formulas) : base(formulas) { }

        // 順序に依らず、要素（Formula）の多重集合が一致するかで等価性を判定
        public static bool operator ==(Formulas a, Formulas b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a.Count != b.Count) return false;

            var countsA = a.GroupBy(x => x)
                           .ToDictionary(g => g.Key, g => g.Count());
            var countsB = b.GroupBy(x => x)
                           .ToDictionary(g => g.Key, g => g.Count());

            if (countsA.Count != countsB.Count) return false;
            foreach (var kv in countsA)
            {
                if (!countsB.TryGetValue(kv.Key, out var c)) return false;
                if (c != kv.Value) return false;
            }
            return true;
        }

        public static bool operator !=(Formulas a, Formulas b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is Formulas other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // 順序非依存のハッシュ。各要素のハッシュと出現回数を組み合わせて XOR 集約。
            int hash = 0;
            foreach (var g in this.GroupBy(x => x))
            {
                unchecked
                {
                    hash ^= HashCode.Combine(g.Key?.GetHashCode() ?? 0, g.Count());
                }
            }
            return hash;
        }

        // Add のオーバーロード: Formula を 1 要素追加
        public new void Add(Formula item)
        {
            base.Add(item);
        }

        // Add のオーバーロード: Formulas をすべて追加（多重集合として結合）
        public void Add(Formulas items)
        {
            if (items is null) return;
            base.AddRange(items);
        }

        // Equals に基づく包含判定（List<T>.Contains は Equals を使うが、明示メソッドを用意）
        public bool ContainsFormula(Formula item)
        {
            return this.Any(f => f == item);
        }

        // 指定要素の出現回数（多重集合のカウント）
        public int CountOf(Formula item)
        {
            return this.Count(f => f == item);
        }

        // 多重集合差分: (this\other, other\this) の 2 つを返す
        public (Formulas OnlyInThis, Formulas OnlyInOther) MultisetExcept(Formulas other)
        {
            if (other is null)
            {
                return (new Formulas(this), new Formulas(Enumerable.Empty<Formula>()));
            }

            var countsA = this.GroupBy(x => x)
                               .ToDictionary(g => g.Key, g => g.Count());
            var countsB = other.GroupBy(x => x)
                               .ToDictionary(g => g.Key, g => g.Count());

            var keys = new HashSet<Formula>(countsA.Keys);
            keys.UnionWith(countsB.Keys);

            var onlyA = new List<Formula>();
            var onlyB = new List<Formula>();

            foreach (var k in keys)
            {
                countsA.TryGetValue(k, out var ca);
                countsB.TryGetValue(k, out var cb);
                var da = ca - cb; // this にのみ残る数
                var db = cb - ca; // other にのみ残る数
                if (da > 0)
                {
                    for (int i = 0; i < da; i++) onlyA.Add(k);
                }
                if (db > 0)
                {
                    for (int i = 0; i < db; i++) onlyB.Add(k);
                }
            }

            return (new Formulas(onlyA), new Formulas(onlyB));
        }

        // 多重集合の共通部分: 各要素の最小出現回数
        public Formulas MultisetIntersect(Formulas other)
        {
            if (other is null) return new Formulas(Enumerable.Empty<Formula>());

            var countsA = this.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var countsB = other.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var result = new List<Formula>();
            foreach (var kv in countsA)
            {
                if (countsB.TryGetValue(kv.Key, out var cb))
                {
                    int n = Math.Min(kv.Value, cb);
                    if (n <= 0) continue; // 共通部分の式の個数が0のものは除外
                    for (int i = 0; i < n; i++) result.Add(kv.Key);
                }
            }
            return new Formulas(result);
        }
    }

    public struct Sequent
    {
        public readonly Formulas Antecedents;
        public readonly Formulas Consequents;

        public Sequent(Formulas antecedents, Formulas consequents)
        {
            Antecedents = antecedents;
            Consequents = consequents;
        }
        public static Sequent Empty;
    }



    public struct Proof
    {
        public readonly Sequent Conclusion;
        public readonly List<Proof> Premises;
        public readonly Rule Rule;
        public bool IsValid => CheckValidity(Rule).Item1;
        public Proof(List<Proof> premises, Sequent conclusion, Rule rule)
        {
            Conclusion = conclusion;
            Premises = premises;
            Rule = rule;
        }
        private (bool, Rule) CheckValidity(Rule rule)
        {

            switch (rule)
            {
                case Rule.I:
                    // Identity rule: A ⊢ A
                    if (Premises.Count != 0) return (false, Rule.I);
                    if (Conclusion.Antecedents.Count != 1 || Conclusion.Consequents.Count != 1) return (false, Rule.I);
                    if (Conclusion.Antecedents[0] != Conclusion.Consequents[0]) return (false, Rule.I);
                    // 任意の論理式 A で A ⊢ A を許可（命題変数に限定しない）
                    return (true, Rule.I);

                // 他のルールの妥当性チェックもここに実装
                case Rule.Cut:
                    // Cut: Γ ⊢ Δ, A  と  A, Π ⊢ Σ から  Γ, Π ⊢ Δ, Σ
                    // 前提が2つで、右側(0番目前提の結論の後件)と左側(1番目前提の結論の前件)に少なくとも1つ共通式があることを要求
                    if (Premises.Count != 2) return (false, Rule.Cut);
                    var common = Premises[0].Conclusion.Consequents.MultisetIntersect(Premises[1].Conclusion.Antecedents);
                    if (common.Count == 0) return (false, Rule.Cut); // 共通部分 0 のものは不成立
                    // 前件: Γ と Π (共通式 A を除いた部分) を統合
                    var (onlyLeftPrem0, onlyCommonDiscard0) = Premises[0].Conclusion.Antecedents.MultisetExcept(common);
                    var (onlyLeftPrem1, onlyCommonDiscard1) = Premises[1].Conclusion.Antecedents.MultisetExcept(common);
                    var expectedAntecedents = new Formulas(onlyLeftPrem0);
                    expectedAntecedents.Add(onlyLeftPrem1);

                    // 後件: Δ と Σ (共通式 A を除いた部分) を統合
                    var (onlyRightPrem0, onlyCommonDiscardRight0) = Premises[0].Conclusion.Consequents.MultisetExcept(common);
                    var (onlyRightPrem1, onlyCommonDiscardRight1) = Premises[1].Conclusion.Consequents.MultisetExcept(common);
                    var expectedConsequents = new Formulas(onlyRightPrem0);
                    expectedConsequents.Add(onlyRightPrem1);

                    if (Conclusion.Antecedents != expectedAntecedents || Conclusion.Consequents != expectedConsequents)
                        return (false, Rule.Cut);
                    return (true, Rule.Cut);
                case Rule.andL:
                    if (Premises.Count != 1) return (false, Rule.andL);
                    var premise = Premises[0].Conclusion;
                    var delta = premise.Antecedents.MultisetExcept(Conclusion.Antecedents);
                    if (!(delta.Item1.Count == 1 && delta.Item2.Count == 1)) return (false, Rule.andL);
                    if (delta.Item2[0] is not And andFormula) return (false, Rule.andL);
                    if (andFormula.Left != delta.Item1[0] && andFormula.Right != delta.Item1[0]) return (false, Rule.andL);
                    return (true, Rule.andL);
                case Rule.andR:
                    if (Premises.Count != 2) return (false, Rule.andR);
                    var preA1 = Premises[0].Conclusion.Antecedents;
                    var preA2 = Premises[1].Conclusion.Antecedents;
                    var preC1 = Premises[0].Conclusion.Consequents;
                    var preC2 = Premises[1].Conclusion.Consequents;
                    preA1.Add(preA2);
                    if (Conclusion.Antecedents != preA1) return (false, Rule.andR);
                    preC1.Add(preC2);
                    var deltaAndR = preC1.MultisetExcept(Conclusion.Consequents);  //((A,B),A∧B)が出るはず
                    if (deltaAndR.Item1.Count != 2 || deltaAndR.Item2.Count != 1) return (false, Rule.andR);
                    if (deltaAndR.Item2[0] is not And andRForm) return (false, Rule.andR);
                    if (!((andRForm.Left == deltaAndR.Item1[0] && andRForm.Right == deltaAndR.Item1[1]) ||
                          (andRForm.Left == deltaAndR.Item1[1] && andRForm.Right == deltaAndR.Item1[0])))
                        return (false, Rule.andR);
                    return (true, Rule.andR);
                case Rule.orR:
                    if (Premises.Count != 1) return (false, Rule.orR);
                    var prem = Premises[0].Conclusion;
                    var deltaOrR = prem.Consequents.MultisetExcept(Conclusion.Consequents);
                    if (deltaOrR.Item1.Count != 1 || deltaOrR.Item2.Count != 1) return (false, Rule.orR);
                    if (deltaOrR.Item2[0] is not Or orFormula) return (false, Rule.orR);
                    if (orFormula.Left != deltaOrR.Item1[0] && orFormula.Right != deltaOrR.Item1[0]) return (false, Rule.orR);
                    return (true, Rule.orR);
                case Rule.orL:
                    // orL: Γ,A ⊢ Δ と Σ,B ⊢ Π から Γ,Σ,A∨B ⊢ Δ,Π
                    if (Premises.Count != 2) return (false, Rule.orL);
                    var orLPre0 = Premises[0].Conclusion;
                    var orLPre1 = Premises[1].Conclusion;

                    // 前提前件の合併と結論前件の差分: ((A,B), A∨B) の形を期待
                    var anteAllOrL = new Formulas(orLPre0.Antecedents);
                    anteAllOrL.Add(orLPre1.Antecedents);
                    var deltaOrL = anteAllOrL.MultisetExcept(Conclusion.Antecedents);
                    if (deltaOrL.Item1.Count != 2 || deltaOrL.Item2.Count != 1) return (false, Rule.orL);
                    if (deltaOrL.Item2[0] is not Or orLForm) return (false, Rule.orL);
                    var aOr = deltaOrL.Item1[0];
                    var bOr = deltaOrL.Item1[1];
                    if (!((orLForm.Left == aOr && orLForm.Right == bOr) || (orLForm.Left == bOr && orLForm.Right == aOr)))
                        return (false, Rule.orL);

                    // 後件: 結論後件は前提後件の合併と一致
                    var consAllOrL = new Formulas(orLPre0.Consequents);
                    consAllOrL.Add(orLPre1.Consequents);
                    if (Conclusion.Consequents != consAllOrL) return (false, Rule.orL);
                    return (true, Rule.orL);
                case Rule.impL:
                    // Implication Left: From Γ ⊢ A,Δ と B, Π ⊢ Σ, から A → B, Γ, Π ⊢ Δ, Σ を導く
                    if (Premises.Count != 2) return (false, Rule.impL);

                    // ① すべての前提の前件・後件を併合した Formulas と、結論の前件・後件を併合した Formulas の差分（多重集合）を取る
                    var premisesAll = new Formulas(Enumerable.Empty<Formula>());
                    premisesAll.Add(Premises[0].Conclusion.Antecedents);
                    premisesAll.Add(Premises[0].Conclusion.Consequents);
                    premisesAll.Add(Premises[1].Conclusion.Antecedents);
                    premisesAll.Add(Premises[1].Conclusion.Consequents);

                    var conclusionAll = new Formulas(Conclusion.Antecedents);
                    conclusionAll.Add(Conclusion.Consequents);

                    var deltaImpL = premisesAll.MultisetExcept(conclusionAll);

                    // ② Item1 の要素数が 2、かつ Item2 の要素数が 1 でなければ弾く
                    if (deltaImpL.Item1.Count != 2 || deltaImpL.Item2.Count != 1) return (false, Rule.impL);

                    // ③ Item2 の唯一の要素が Implication でなければ弾く
                    if (deltaImpL.Item2[0] is not Implication impl) return (false, Rule.impL);

                    // 非可換性考慮: impl.Left は前提の後件に、impl.Right は前提の前件に含まれていたはず
                    var premisesAnteAll = new Formulas(Premises[0].Conclusion.Antecedents);
                    premisesAnteAll.Add(Premises[1].Conclusion.Antecedents);
                    var premisesConsAll = new Formulas(Premises[0].Conclusion.Consequents);
                    premisesConsAll.Add(Premises[1].Conclusion.Consequents);

                    // delta の 2 つが {impl.Left, impl.Right} と一致し、かつ方向が正しいこと（== で直接比較）
                    var d0 = deltaImpL.Item1[0];
                    var d1 = deltaImpL.Item1[1];
                    if (!(d0 == impl.Left && d1 == impl.Right)) return (false, Rule.impL);
                    if (!premisesConsAll.ContainsFormula(impl.Left)) return (false, Rule.impL);
                    if (!premisesAnteAll.ContainsFormula(impl.Right)) return (false, Rule.impL);

                    // 以降は他の Rule 実装に倣い、期待される前件・後件を構成して一致を確認
                    // 期待前件 = (前提前件の併合から impl.Right を 1 つ除去) + {impl}
                    var (anteWithoutB, _) = premisesAnteAll.MultisetExcept(new Formulas(new[] { impl.Right }));
                    var expectedAnte = new Formulas(anteWithoutB);
                    expectedAnte.Add(new Formulas(new Formula[] { impl }));

                    // 期待後件 = 前提後件の併合から impl.Left を 1 つ除去
                    var (consWithoutA, _) = premisesConsAll.MultisetExcept(new Formulas(new[] { impl.Left }));
                    var expectedCons = new Formulas(consWithoutA);

                    if (Conclusion.Antecedents != expectedAnte) return (false, Rule.impL);
                    if (Conclusion.Consequents != expectedCons) return (false, Rule.impL);

                    return (true, Rule.impL);
                case Rule.impR:
                    // Implication Right: 前提 Γ, A ⊢ B, Δ から 結論 Γ ⊢ A→B, Δ を導く（ユーザー仕様①〜④に従う）
                    if (Premises.Count != 1) return (false, Rule.impR);
                    var impRPrem = Premises[0].Conclusion;

                    // ① 前提(前件+後件)と結論(前件+後件)の多重集合差分を取得
                    var premAll = new Formulas(Enumerable.Empty<Formula>());
                    premAll.Add(impRPrem.Antecedents);
                    premAll.Add(impRPrem.Consequents);
                    var concAll = new Formulas(Conclusion.Antecedents);
                    concAll.Add(Conclusion.Consequents);
                    var deltaImpR = premAll.MultisetExcept(concAll);

                    // ② 差分の Item1 が2要素、Item2 が1要素でその唯一の要素が Implication でなければ弾く
                    if (deltaImpR.Item1.Count != 2) return (false, Rule.impR);
                    if (deltaImpR.Item2.Count != 1 || deltaImpR.Item2[0] is not Implication impRFormula) return (false, Rule.impR);

                    // ③ Implication の Left/Right が Item1 の2要素と順序一致（非可換）
                    var aDelta = deltaImpR.Item1[0];
                    var bDelta = deltaImpR.Item1[1];
                    if (!(impRFormula.Left == aDelta && impRFormula.Right == bDelta)) return (false, Rule.impR);

                    // ④ 結論前件 = 前提前件から Left を1つ除去
                    //    結論後件 = 前提後件から Right を1つ除去し Implication を1つ追加
                    var (anteWithoutA, _) = impRPrem.Antecedents.MultisetExcept(new Formulas(new[] { aDelta }));
                    var (consWithoutB, _) = impRPrem.Consequents.MultisetExcept(new Formulas(new[] { bDelta }));
                    var expectedAnteImpR = new Formulas(anteWithoutA);
                    var expectedConsImpR = new Formulas(consWithoutB);
                    expectedConsImpR.Add(new Formulas(new Formula[] { impRFormula }));

                    if (Conclusion.Antecedents != expectedAnteImpR) return (false, Rule.impR);
                    if (Conclusion.Consequents != expectedConsImpR) return (false, Rule.impR);
                    return (true, Rule.impR);
                case Rule.notL:
                    // notL: 前提 Γ ⊢ Δ, A から 結論 ¬A, Γ ⊢ Δ を導く
                    // 仕様: ①前提1つ ②差分取得 ③差分 Item1/Item2 とも1要素 ④Item2 が Not で Operand が Item1 ⑤結論前件/後件が期待形
                    if (Premises.Count != 1) return (false, Rule.notL); // ①
                    var notLPrem = Premises[0].Conclusion;

                    // ② 前提の(前件+後件) vs 結論の(前件+後件) 差分
                    var notLPremAll = new Formulas(Enumerable.Empty<Formula>());
                    notLPremAll.Add(notLPrem.Antecedents);
                    notLPremAll.Add(notLPrem.Consequents);
                    var notLConcAll = new Formulas(Conclusion.Antecedents);
                    notLConcAll.Add(Conclusion.Consequents);
                    var deltaNotL = notLPremAll.MultisetExcept(notLConcAll);

                    // ③ 差分要素数チェック
                    if (deltaNotL.Item1.Count != 1 || deltaNotL.Item2.Count != 1) return (false, Rule.notL);

                    // ④ Item2 が Not で、Operand が Item1 と一致
                    if (deltaNotL.Item2[0] is not Not notFormula) return (false, Rule.notL);
                    var operandL = deltaNotL.Item1[0];
                    if (notFormula.Operand != operandL) return (false, Rule.notL);

                    // ⑤ 結論前件 = 前提前件 + Not, 結論後件 = 前提後件 - Operand
                    var (anteRemovedOperandL, _) = notLPrem.Antecedents.MultisetExcept(new Formulas(Enumerable.Empty<Formula>())); // no removal
                    var expectedAnteNotL = new Formulas(anteRemovedOperandL);
                    expectedAnteNotL.Add(new Formulas(new Formula[] { notFormula }));
                    var (consWithoutOperandL, _) = notLPrem.Consequents.MultisetExcept(new Formulas(new[] { operandL }));
                    var expectedConsNotL = new Formulas(consWithoutOperandL);

                    if (Conclusion.Antecedents != expectedAnteNotL) return (false, Rule.notL);
                    if (Conclusion.Consequents != expectedConsNotL) return (false, Rule.notL);
                    return (true, Rule.notL);
                case Rule.notR:
                    // notR: 前提 Γ, A ⊢ Δ から 結論 Γ ⊢ Δ, ¬A を導く
                    // 仕様: ①前提1つ ②差分取得 ③差分 Item1/Item2 とも1要素 ④Item2 が Not で Operand が Item1 ⑤結論前件/後件が期待形
                    if (Premises.Count != 1) return (false, Rule.notR); // ①
                    var notRPrem = Premises[0].Conclusion;

                    // ② 差分
                    var notRPremAll = new Formulas(Enumerable.Empty<Formula>());
                    notRPremAll.Add(notRPrem.Antecedents);
                    notRPremAll.Add(notRPrem.Consequents);
                    var notRConcAll = new Formulas(Conclusion.Antecedents);
                    notRConcAll.Add(Conclusion.Consequents);
                    var deltaNotR = notRPremAll.MultisetExcept(notRConcAll);

                    // ③ 要素数チェック
                    if (deltaNotR.Item1.Count != 1 || deltaNotR.Item2.Count != 1) return (false, Rule.notR);

                    // ④ Not/Operand チェック
                    if (deltaNotR.Item2[0] is not Not notRFormula) return (false, Rule.notR);
                    var operandR = deltaNotR.Item1[0];
                    if (notRFormula.Operand != operandR) return (false, Rule.notR);

                    // ⑤ 結論前件 = 前提前件 - Operand, 結論後件 = 前提後件 + Not
                    var (anteWithoutOperandR, _) = notRPrem.Antecedents.MultisetExcept(new Formulas(new[] { operandR }));
                    var expectedAnteNotR = new Formulas(anteWithoutOperandR);
                    var (consRemovedOperandR, _) = notRPrem.Consequents.MultisetExcept(new Formulas(Enumerable.Empty<Formula>())); // no removal
                    var expectedConsNotR = new Formulas(consRemovedOperandR);
                    expectedConsNotR.Add(new Formulas(new Formula[] { notRFormula }));

                    if (Conclusion.Antecedents != expectedAnteNotR) return (false, Rule.notR);
                    if (Conclusion.Consequents != expectedConsNotR) return (false, Rule.notR);
                    return (true, Rule.notR);
                case Rule.WL:
                    // Weakening Left: 結論前件は前提前件の多重集合スーパーセット、後件は完全一致
                    if (Premises.Count != 1) return (false, Rule.WL);
                    var wlPrem = Premises[0].Conclusion;
                    if (Conclusion.Consequents != wlPrem.Consequents) return (false, Rule.WL);
                    var diffWL = wlPrem.Antecedents.MultisetExcept(Conclusion.Antecedents);
                    // 取りこぼし無し（OnlyInThis=0）、かつ 何か追加あり（OnlyInOther>=1）
                    if (diffWL.OnlyInThis.Count != 0) return (false, Rule.WL);
                    if (diffWL.OnlyInOther.Count < 1) return (false, Rule.WL);
                    return (true, Rule.WL);
                case Rule.WR:
                    // Weakening Right: 結論後件は前提後件の多重集合スーパーセット、前件は完全一致
                    if (Premises.Count != 1) return (false, Rule.WR);
                    var wrPrem = Premises[0].Conclusion;
                    if (Conclusion.Antecedents != wrPrem.Antecedents) return (false, Rule.WR);
                    var diffWR = wrPrem.Consequents.MultisetExcept(Conclusion.Consequents);
                    if (diffWR.OnlyInThis.Count != 0) return (false, Rule.WR);
                    if (diffWR.OnlyInOther.Count < 1) return (false, Rule.WR);
                    return (true, Rule.WR);
                case Rule.CL:
                    // Contraction Left: 前件で同一式が2つ以上 → 結論でその式が1つ、他は不変、右辺不変
                    if (Premises.Count != 1) return (false, Rule.CL);
                    var clPrem = Premises[0].Conclusion;
                    if (Conclusion.Consequents != clPrem.Consequents) return (false, Rule.CL);
                    // 候補式: premで2つ以上、concで1つ
                    var clAll = clPrem.Antecedents.Concat(Conclusion.Antecedents).Distinct().ToList();
                    var clConc = Conclusion; // 構造体 this のキャプチャ回避用ローカル
                    var clCandidates = clAll.Where(f => clPrem.Antecedents.CountOf(f) >= 2 && clConc.Antecedents.CountOf(f) == 1).ToList();
                    if (clCandidates.Count != 1) return (false, Rule.CL);
                    var clF = clCandidates[0];
                    // 他要素はカウント同じ
                    foreach (var f in clAll)
                    {
                        if (f == clF) continue;
                        if (clPrem.Antecedents.CountOf(f) != clConc.Antecedents.CountOf(f)) return (false, Rule.CL);
                    }
                    // サイズ差は premでの clF の余剰分（Count-1）だけ減っている
                    int premCountF = clPrem.Antecedents.CountOf(clF);
                    if (clPrem.Antecedents.Count - clConc.Antecedents.Count != premCountF - 1) return (false, Rule.CL);
                    return (true, Rule.CL);
                case Rule.CR:
                    // Contraction Right: 後件で同一式が2つ以上 → 結論でその式が1つ、他は不変、左辺不変
                    if (Premises.Count != 1) return (false, Rule.CR);
                    var crPrem = Premises[0].Conclusion;
                    if (Conclusion.Antecedents != crPrem.Antecedents) return (false, Rule.CR);
                    var crAll = crPrem.Consequents.Concat(Conclusion.Consequents).Distinct().ToList();
                    var crConc = Conclusion; // 構造体 this のキャプチャ回避用ローカル
                    var crCandidates = crAll.Where(f => crPrem.Consequents.CountOf(f) >= 2 && crConc.Consequents.CountOf(f) == 1).ToList();
                    if (crCandidates.Count != 1) return (false, Rule.CR);
                    var crF = crCandidates[0];
                    foreach (var f in crAll)
                    {
                        if (f == crF) continue;
                        if (crPrem.Consequents.CountOf(f) != crConc.Consequents.CountOf(f)) return (false, Rule.CR);
                    }
                    int premCountFr = crPrem.Consequents.CountOf(crF);
                    if (crPrem.Consequents.Count - crConc.Consequents.Count != premCountFr - 1) return (false, Rule.CR);
                    return (true, Rule.CR);
                default:
                    return (false, Rule.I); // 未実装のルールは不正とする
            }
        }

    }

    public class ProofTree
    {
        public readonly Proof Root;
        public ProofTree(Proof root)
        {
            Root = root;
        }

        public bool IsValid()
        {
            return IsValidRecursive(Root);
        }

        private bool IsValidRecursive(Proof node)
        {
            // 現在の Proof.IsValid 判定（局所妥当性）が偽なら即座に偽
            if (!node.IsValid) return false;

            // 前提が無ければ葉として妥当
            if (node.Premises == null || node.Premises.Count == 0) return true;

            // すべての前提が再帰的に妥当か確認
            foreach (var p in node.Premises)
            {
                if (!IsValidRecursive(p)) return false;
            }
            return true;
        }
    }


    public enum Rule
    {
        I,
        Cut,
        andL,
        andR,
        orL,
        orR,
        impL,
        impR,
        notL,
        notR,
        WL,
        WR,
        CL,
        CR,
        //他のルールもここに追加可能
    }

    public enum Kind
    {
        Variable,
        Not,
        And,
        Or,
        Implication
    }

    /// <summary>
    /// 前提シーケント列と指定ルールから導出可能な結論シーケント候補を列挙するユーティリティ。
    /// （既存の妥当性チェックロジックを反転し、生成方向を実装）
    /// 注意: Weakening(WL/WR) は新しい式を任意追加するため列挙不能として空集合を返す。
    /// </summary>
    public static class LKInference
    {
        public static IEnumerable<Sequent> EnumerateConclusions(IReadOnlyList<Sequent> premises, Rule rule)
        {
            // 任意性のあるルール（サジェスト対象外）
            // - orR: 後件の元素を X∨Y の形に拡張する際、Y の選択が任意
            // - andL: 前件の A∧B を A または B に置き換える際、選択が任意
            // - WL/WR: 新しい式を任意追加するため列挙不能
            if (rule == Rule.orR || rule == Rule.andL || rule == Rule.WL || rule == Rule.WR)
            {
                return Enumerable.Empty<Sequent>();
            }

            switch (rule)
            {
                case Rule.I:
                    // 任意の式 A に対し A ⊢ A を生成。前提不要なので列挙不能 -> 空 or 全変数? ここでは空 (外部で明示生成)。
                    return Enumerable.Empty<Sequent>();
                case Rule.andR:
                    return InferAndR(premises);
                case Rule.orL:
                    return InferOrL(premises);
                case Rule.impL:
                    return InferImpL(premises);
                case Rule.impR:
                    return InferImpR(premises);
                case Rule.notL:
                    return InferNotL(premises);
                case Rule.notR:
                    return InferNotR(premises);
                case Rule.Cut:
                    return InferCut(premises);
                case Rule.CL:
                    return InferCL(premises);
                case Rule.CR:
                    return InferCR(premises);
                default:
                    return Enumerable.Empty<Sequent>();
            }
        }

        /// <summary>
        /// 与えられた前提シーケント列と結論シーケントから、局所的に妥当な規則候補を列挙する。
        /// Proof.CheckValidity を全ての Rule について試し、妥当なもののみ返す。
        /// 
        /// ただし、以下の任意性のあるルールは除外される:
        /// - orR: 後件の元素を任意の X∨Y に拡張可能（Y が不定）
        /// - andL: 前件の A∧B を A または B のいずれかに置き換え可能（選択が不定）
        /// - WL/WR: 新しい式を任意追加可能（追加内容が不定）
        /// </summary>
        public static IEnumerable<Rule> EnumerateRules(IReadOnlyList<Sequent> premises, Sequent conclusion)
        {
            // 各前提シーケントからダミーの葉 Proof を作る（ルール I で仮置き）
            var premiseProofs = premises
                .Select(s => new Proof(new List<Proof>(), s, Rule.I))
                .ToList();

            foreach (Rule rule in Enum.GetValues(typeof(Rule)))
            {
                // 任意性のあるルールは除外
                if (rule == Rule.orR || rule == Rule.andL || rule == Rule.WL || rule == Rule.WR)
                {
                    continue;
                }

                var proof = new Proof(premiseProofs, conclusion, rule);
                if (proof.IsValid)
                {
                    yield return rule;
                }
            }
        }

        /// <summary>
        /// 前提シーケント列と結論シーケントから、妥当な規則がちょうど1つに絞れる場合にそれを返す。
        /// 0個または2個以上マッチする場合は null を返す。
        /// </summary>
        public static Rule? InferUniqueRule(IReadOnlyList<Sequent> premises, Sequent conclusion)
        {
            Rule? found = null;
            foreach (var r in EnumerateRules(premises, conclusion))
            {
                if (found.HasValue)
                {
                    // 2個以上マッチしたので一意でない
                    return null;
                }
                found = r;
            }
            return found;
        }

        private static IEnumerable<Sequent> InferAndL(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var antecedents = p.Antecedents;
            var consequents = p.Consequents;
            var results = new List<Sequent>();
            foreach (var f in antecedents)
            {
                if (f is And andF)
                {
                    // 置換: A∧B -> A と A∧B -> B
                    results.Add(CreateReplaceOne(antecedents, consequents, andF, andF.Left, true));
                    results.Add(CreateReplaceOne(antecedents, consequents, andF, andF.Right, true));
                }
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferAndR(IReadOnlyList<Sequent> premises)
        {
            // 前提から2つを選ぶすべての組み合わせで InferAndR を実行
            var results = new List<Sequent>();
            
            foreach (var combo in Combinations(premises.Count, 2))
            {
                var p0 = premises[combo[0]];
                var p1 = premises[combo[1]];
                var ante = new List<Formula>(p0.Antecedents);
                ante.AddRange(p1.Antecedents);
                
                foreach (var a in p0.Consequents)
                {
                    foreach (var b in p1.Consequents)
                    {
                        var cons = new List<Formula>(p0.Consequents);
                        cons.AddRange(p1.Consequents);
                        // A,B を 1 つずつ除去
                        RemoveOnce(cons, a);
                        RemoveOnce(cons, b);
                        cons.Add(new And(a, b));
                        results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
                    }
                }
            }
            
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferOrR(IReadOnlyList<Sequent> premises)
        {
            // 前提 Γ ⊢ Δ から Δ の任意要素 X を X∨Y に拡張するには Y の選択が必要。ここでは X∨X の形のみ列挙。
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            foreach (var x in p.Consequents)
            {
                var cons = new List<Formula>(p.Consequents);
                RemoveOnce(cons, x);
                cons.Add(new Or(x, x));
                results.Add(new Sequent(new Formulas(p.Antecedents), new Formulas(cons)));
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferOrL(IReadOnlyList<Sequent> premises)
        {
            // 前提から2つを選ぶすべての組み合わせで InferOrL を実行
            var results = new List<Sequent>();
            
            foreach (var combo in Combinations(premises.Count, 2))
            {
                var p0 = premises[combo[0]];
                var p1 = premises[combo[1]];
                var cons = new List<Formula>(p0.Consequents);
                cons.AddRange(p1.Consequents);
                
                foreach (var a in p0.Antecedents)
                {
                    foreach (var b in p1.Antecedents)
                    {
                        var ante = new List<Formula>(p0.Antecedents);
                        ante.AddRange(p1.Antecedents);
                        // A,B を除去し A∨B 追加
                        RemoveOnce(ante, a);
                        RemoveOnce(ante, b);
                        ante.Add(new Or(a, b));
                        results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
                    }
                }
            }
            
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferImpL(IReadOnlyList<Sequent> premises)
        {
            // 前提から2つを選ぶすべての組み合わせで InferImpL を実行
            var results = new List<Sequent>();
            
            foreach (var combo in Combinations(premises.Count, 2))
            {
                var p0 = premises[combo[0]];
                var p1 = premises[combo[1]];
                var anteAll = new List<Formula>(p0.Antecedents);
                anteAll.AddRange(p1.Antecedents);
                var consAll = new List<Formula>(p0.Consequents);
                consAll.AddRange(p1.Consequents);
                
                // p0の後件の各式とp1の前件の各式のペアで A→B を作る
                foreach (var A in p0.Consequents)
                {
                    foreach (var B in p1.Antecedents)
                    {
                        var ante = new List<Formula>(anteAll);
                        var cons = new List<Formula>(consAll);
                        // Remove B from ante, A from cons, add A->B to ante
                        if (!RemoveOnce(ante, B)) continue;
                        if (!RemoveOnce(cons, A)) continue;
                        ante.Add(new Implication(A, B));
                        results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
                    }
                }
            }
            
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferImpR(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            foreach (var A in p.Antecedents)
            {
                foreach (var B in p.Consequents)
                {
                    var ante = new List<Formula>(p.Antecedents);
                    var cons = new List<Formula>(p.Consequents);
                    if (!RemoveOnce(ante, A)) continue;
                    if (!RemoveOnce(cons, B)) continue;
                    cons.Add(new Implication(A, B));
                    results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
                }
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferNotL(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            foreach (var A in p.Consequents)
            {
                var ante = new List<Formula>(p.Antecedents) { new Not(A) };
                var cons = new List<Formula>(p.Consequents);
                if (!RemoveOnce(cons, A)) continue;
                results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferNotR(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            foreach (var A in p.Antecedents)
            {
                var ante = new List<Formula>(p.Antecedents);
                if (!RemoveOnce(ante, A)) continue;
                var cons = new List<Formula>(p.Consequents) { new Not(A) };
                results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferCut(IReadOnlyList<Sequent> premises)
        {
            // 前提から2つを選ぶすべての組み合わせで InferCut を実行
            var results = new List<Sequent>();
            
            foreach (var combo in Combinations(premises.Count, 2))
            {
                var p0 = premises[combo[0]];
                var p1 = premises[combo[1]];
                
                // 共通式: 右側(第一前提後件) ∩ 左側(第二前提前件) 多重集合の共通部分
                var common = p0.Consequents.MultisetIntersect(p1.Antecedents);
                
                if (common.Count == 0) continue;
                
                // 共通式の部分集合すべてに対して Cut を試みる（空集合を除く）
                // 共通式の部分集合を列挙（1個以上）
                var commonList = common.ToList();
                int subsetCount = 1 << commonList.Count; // 2^n 個の部分集合
                
                for (int mask = 1; mask < subsetCount; mask++) // mask=0 は空集合なのでスキップ
                {
                    var cutFormulas = new List<Formula>();
                    for (int i = 0; i < commonList.Count; i++)
                    {
                        if ((mask & (1 << i)) != 0)
                        {
                            cutFormulas.Add(commonList[i]);
                        }
                    }
                    
                    // cutFormulas を除去したシーケントを構成
                    var ante = new List<Formula>(p0.Antecedents);
                    var ante2 = new List<Formula>(p1.Antecedents);
                    
                    // cutFormulas を ante2 から除去
                    foreach (var f in cutFormulas)
                    {
                        RemoveOnce(ante2, f);
                    }
                    ante.AddRange(ante2);
                    
                    var cons = new List<Formula>(p0.Consequents);
                    
                    // cutFormulas を cons から除去
                    foreach (var f in cutFormulas)
                    {
                        RemoveOnce(cons, f);
                    }
                    cons.AddRange(p1.Consequents);
                    
                    results.Add(new Sequent(new Formulas(ante), new Formulas(cons)));
                }
            }
            
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferCL(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            var distinct = p.Antecedents.Distinct().ToList();
            foreach (var f in distinct)
            {
                int count = p.Antecedents.CountOf(f);
                if (count >= 2)
                {
                    var ante = new List<Formula>(p.Antecedents);
                    // 全削除→1つ追加
                    ante.RemoveAll(x => x == f);
                    ante.Add(f);
                    results.Add(new Sequent(new Formulas(ante), new Formulas(p.Consequents)));
                }
            }
            return Distinct(results);
        }

        private static IEnumerable<Sequent> InferCR(IReadOnlyList<Sequent> premises)
        {
            if (premises.Count != 1) return Enumerable.Empty<Sequent>();
            var p = premises[0];
            var results = new List<Sequent>();
            var distinct = p.Consequents.Distinct().ToList();
            foreach (var f in distinct)
            {
                int count = p.Consequents.CountOf(f);
                if (count >= 2)
                {
                    var cons = new List<Formula>(p.Consequents);
                    cons.RemoveAll(x => x == f);
                    cons.Add(f);
                    results.Add(new Sequent(new Formulas(p.Antecedents), new Formulas(cons)));
                }
            }
            return Distinct(results);
        }

        private static Sequent CreateReplaceOne(Formulas ante, Formulas cons, Formula target, Formula replacement, bool inAntecedent)
        {
            var newAnte = new List<Formula>(ante);
            if (inAntecedent)
            {
                if (RemoveOnce(newAnte, target)) newAnte.Add(replacement);
            }
            var newCons = new List<Formula>(cons);
            return new Sequent(new Formulas(newAnte), new Formulas(newCons));
        }

        private static bool RemoveOnce(List<Formula> list, Formula f)
        {
            int idx = list.FindIndex(x => x == f);
            if (idx < 0) return false;
            list.RemoveAt(idx);
            return true;
        }

        /// <summary>
        /// n個の要素からk個を選ぶ組み合わせを生成する。
        /// 例: n=4, k=2 -> [0,1], [0,2], [0,3], [1,2], [1,3], [2,3] (6通り)
        /// </summary>
        private static IEnumerable<List<int>> Combinations(int n, int k)
        {
            if (k > n || k < 0) yield break;
            if (k == 0)
            {
                yield return new List<int>();
                yield break;
            }
            
            var result = new List<int>();
            var indices = new int[k];
            for (int i = 0; i < k; i++) indices[i] = i;
            
            while (true)
            {
                yield return new List<int>(indices);
                
                // 次の組み合わせを生成
                int i = k - 1;
                while (i >= 0 && indices[i] == n - k + i) i--;
                if (i < 0) break;
                
                indices[i]++;
                for (int j = i + 1; j < k; j++) indices[j] = indices[j - 1] + 1;
            }
        }

        private static IEnumerable<Sequent> Distinct(IEnumerable<Sequent> seqs)
        {
            var seen = new HashSet<(int, int)>();
            foreach (var s in seqs)
            {
                int h = s.Antecedents.GetHashCode();
                int h2 = s.Consequents.GetHashCode();
                if (seen.Add((h, h2))) yield return s;
            }
        }
    }

}