// Assets/Scripts/UI/ProfileItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace DebtJam
{
    /// <summary>
    /// 仅负责【展示】一个欠债人：头像、标题、若干词条（模糊/划线纠正）。
    /// 新增：crossOut（打叉/灰显）。遵循最新规则：结局后 Profile 列表中将移除该项；
    /// 若你想在移除前给一个“打叉动画”提示，可先 PlayCrossOutThenHide() 再由 ProfilePanel 刷新。
    /// </summary>
    public class ProfileItemDisplay_TMP : MonoBehaviour
    {
        [Header("UI")]
        public Image portrait;
        public TMP_Text title;     // 例：David | $ 3000
        public TMP_Text facts;     // 多行词条（支持 <s>、<alpha>）
        [Tooltip("打叉/灰显 叠层（可放置半透明X、蒙版等）")]
        public GameObject crossOut;

        [Header("显示上限")]
        public int maxFactsToShow = 3;

        // —— 外部使用：填充数据（保持你原有调用不变） ——————————
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
                facts.richText = true;
                facts.text = BuildFactsPreview(rt);
            }

            // 默认不显示打叉（是否显示交由 ProfilePanel 控制）
            if (crossOut) crossOut.SetActive(false);
        }

        // —— 新增：立即切换打叉状态（不做动画） ——————————
        public void SetCrossOut(bool on)
        {
            if (crossOut) crossOut.SetActive(on);
        }

        // —— 新增：打叉提示后自动隐藏（用于“结局后从列表移除”前的过渡） ——————————
        public void PlayCrossOutThenHide(float delay = 0.6f)
        {
            if (!gameObject.activeInHierarchy)
            {
                // 若物体不激活，直接隐藏即可
                gameObject.SetActive(false);
                return;
            }
            if (crossOut) crossOut.SetActive(true);
            StartCoroutine(CoHide(delay));
        }

        System.Collections.IEnumerator CoHide(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }

        // —— 内部：构建词条预览（保持你的逻辑不变） ——————————
        private static readonly string[] kOrder = { "Name", "Phone", "Address" };
        private static int GetPriority(string key)
        {
            for (int i = 0; i < kOrder.Length; i++)
                if (string.Equals(kOrder[i], key, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            return 999;
        }

        private string BuildFactsPreview(CaseRuntime rt)
        {
            var sb = new System.Text.StringBuilder();

            var ordered = rt.facts.Values
                .OrderBy(f => f.state == FactState.Unknown) // Unknown 放后
                .ThenBy(f => GetPriority(f.key))
                .ThenBy(f => f.label)
                .Take(maxFactsToShow);

            foreach (var f in ordered)
            {
                if (f.state == FactState.Unknown)
                    sb.AppendLine($"{f.label}: <alpha=#55>████（模糊）</alpha>");
                else if (!string.IsNullOrEmpty(f.oldValueStriked))
                    sb.AppendLine($"{f.label}: <s>{f.oldValueStriked}</s>  {f.value}");
                else
                    sb.AppendLine($"{f.label}: {f.value}");
            }
            if (rt.facts.Count > maxFactsToShow) sb.AppendLine("…");
            return sb.ToString();
        }
    }
}
