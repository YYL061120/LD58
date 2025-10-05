using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Text;

namespace DebtJam
{
    /// <summary>
    /// 展示单个欠债人：头像、标题、若干词条（含模糊/划线纠正）；无交互。
    /// 使用 TextMeshPro（TMP_Text）。
    /// </summary>
    public class ProfileItemDisplay_TMP : MonoBehaviour
    {
        [Header("UI")]
        public Image portrait;
        public TMP_Text title;   // 例：David  欠款：$3000（Pending）
        public TMP_Text facts;   // 多行词条

        [Header("显示上限")]
        public int maxFactsToShow = 3;

        public void Setup(CaseRuntime rt, DebtorProfileSO so)
        {
            if (portrait) portrait.sprite = so.portrait;

            if (title)
            {
                title.richText = true;
                title.text = $"{rt.displayName} | $ {rt.amountOwed:n0}";
            }

            if (facts)
            {
                facts.richText = true;       // 允许 <s> 和 <alpha> 标签
                facts.text = BuildFactsPreview(rt);
            }
        }

        private string BuildFactsPreview(CaseRuntime rt)
        {
            var sb = new StringBuilder();

            var ordered = rt.facts.Values
                .OrderBy(f => f.state == FactState.Unknown) // Unknown 放后
                .ThenBy(f => f.label)
                .Take(maxFactsToShow);

            int count = 0;
            foreach (var f in ordered)
            {
                if (f.state == FactState.Unknown)
                {
                    sb.AppendLine($"{f.label}: <alpha=#55>████（模糊）</alpha>");
                }
                else if (!string.IsNullOrEmpty(f.oldValueStriked))
                {
                    sb.AppendLine($"{f.label}: <s>{f.oldValueStriked}</s>  {f.value}");
                }
                else
                {
                    // 如果想把“已知假”标个颜色/标签，也可以：
                    // var tag = f.state == FactState.Fake ? "（假）" : "";
                    // sb.AppendLine($"{f.label}: {f.value}{tag}");
                    sb.AppendLine($"{f.label}: {f.value}");
                }
                count++;
            }

            if (rt.facts.Count > count) sb.AppendLine("…");
            return sb.ToString();
        }
    }
}
