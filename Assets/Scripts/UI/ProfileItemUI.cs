using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;

namespace DebtJam
{
    /// <summary>
    /// 用于展示一个欠款人摘要：头像、姓名、金额、若干词条（含模糊/划线纠正），并支持点击选择。
    /// 需要挂在 itemPrefab 上（带 Button）。
    /// </summary>
    public class ProfileItemUI : MonoBehaviour
    {
        [Header("UI Refs")]
        public Image portrait;
        public Text title;     // 显示：姓名（Pending/DeadEnd/Collected） + 金额
        public Text facts;     // 显示若干词条的小段文本
        public Image highlight;// 可选：一个淡色高亮底图

        [Header("配置")]
        [Tooltip("最多展示几条词条，避免条目过长")]
        public int maxFactsToShow = 3;

        public CaseRuntime Runtime { get; private set; }
        private System.Action<CaseRuntime> _onClick;

        void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => { if (Runtime != null) _onClick?.Invoke(Runtime); });
        }

        /// <summary>初始化条目</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, System.Action<CaseRuntime> onClick)
        {
            Runtime = rt;
            _onClick = onClick;

            if (portrait) portrait.sprite = so.portrait;

            if (title)
            {
                title.text = $"{rt.displayName}（{rt.outcome}）  欠款：${rt.amountOwed}";
            }

            if (facts)
                facts.text = BuildFactsPreview(rt);

            SetHighlight(false);
        }

        /// <summary>设置是否高亮（当前选中）</summary>
        public void SetHighlight(bool on)
        {
            if (highlight) highlight.enabled = on;
            // 也可以改文本颜色等，这里简单处理
        }

        /// <summary>把若干词条拼成预览文本：Unknown 显示“模糊条”；修正显示 <s>旧</s> 新。</summary>
        private string BuildFactsPreview(CaseRuntime rt)
        {
            var sb = new StringBuilder();

            // 选几条有代表性的词条（先显示 KnownTrue / Fake，再 Unknown）
            var ordered = rt.facts.Values
                .OrderByDescending(f => f.visibility == FactVisibility.Unknown) // Unknown 放后面
                .ThenBy(f => f.label)
                .Take(maxFactsToShow);

            int count = 0;
            foreach (var f in ordered)
            {
                if (f.visibility == FactVisibility.Unknown)
                {
                    sb.AppendLine($"{f.label}: <alpha=#55>████（模糊）</alpha>");
                }
                else if (!string.IsNullOrEmpty(f.oldValueStriked))
                {
                    sb.AppendLine($"{f.label}: <s>{f.oldValueStriked}</s>  {f.value}");
                }
                else
                {
                    sb.AppendLine($"{f.label}: {f.value}");
                }
                count++;
            }

            if (rt.facts.Count > count)
                sb.AppendLine("…");

            return sb.ToString();
        }
    }
}
