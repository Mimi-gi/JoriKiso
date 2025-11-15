using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FormalSystem.LK;
using UnityEngine.UI;

public class ExpectNodeViewer : MonoBehaviour
{
    [Header("ScrollView Content")]
    [SerializeField] RectTransform content; // ScrollView の Content を割り当て

    [Header("Prefabs")]
    [SerializeField] SequentNode sequentNodePrefab; // シーケント表示用プレハブ

    [Header("Options")]
    [SerializeField] bool clearOnRefresh = true; // リフレッシュ時に既存要素を破棄

    // 外部API: 前提と規則から結論候補のシーケントノードを生成してスクロールに格納（毎回リフレッシュ）
    public void RefreshFromPremisesAndRule(IReadOnlyList<Sequent> premises, Rule rule)
    {
        if (content == null || sequentNodePrefab == null)
        {
            Debug.LogError("ExpectNodeViewer: content / sequentNodePrefab が未設定です。");
            return;
        }

        if (clearOnRefresh)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        var conclusions = LKInference.EnumerateConclusions(premises ?? new List<Sequent>(), rule) ?? Enumerable.Empty<Sequent>();
        int created = 0;
        foreach (var seq in conclusions)
        {
            var node = NodeCreator.CreateSequentNode(seq, sequentNodePrefab, content);
            if (node == null) continue;
            PreparePreviewNode(node);
            created++;
        }

        // レイアウトを持っていれば任せる。無ければ最低限のサイズ調整
        var layout = content.GetComponent<LayoutGroup>();
        if (layout == null)
        {
            // 子要素数ぶんだけ高さを確保（各要素高さ=推定32+余白）
            float h = Mathf.Max(0, created) * 40f;
            content.sizeDelta = new Vector2(content.sizeDelta.x, h);
        }
    }

    // プレビュー用途の微調整（必要に応じて無効化やスケール設定）
    void PreparePreviewNode(SequentNode node)
    {
        var rt = node.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }
        // ここでドラッグを無効化/別のドラッグスクリプト付与など将来拡張可能
    }
}
