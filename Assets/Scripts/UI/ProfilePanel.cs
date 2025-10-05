using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

namespace DebtJam
{
    public class ProfilePanel : MonoBehaviour
    {
        public Image portrait;
        public Text header;
        public Text facts;

        void OnEnable() { CaseManager.I.OnCaseChanged += Refresh; Refresh(CaseManager.I.currentCase); }
        void OnDisable() { CaseManager.I.OnCaseChanged -= Refresh; }

        public void Refresh(CaseRuntime rt)
        {
            if (rt == null) return;
            var so = CaseManager.I.GetSO(rt.debtorId);
            if (portrait) portrait.sprite = so.portrait;
            if (header) header.text = $"{rt.displayName}（{rt.outcome}） 欠款：${rt.amountOwed}";

            if (facts)
            {
                var sb = new StringBuilder();
                foreach (var f in rt.facts.Values.OrderBy(v => v.label))
                {
                    if (f.visibility == FactVisibility.Unknown)
                        sb.AppendLine($"{f.label}: <alpha=#55>████（模糊）</alpha>");
                    else if (!string.IsNullOrEmpty(f.oldValueStriked))
                        sb.AppendLine($"{f.label}: <s>{f.oldValueStriked}</s>  {f.value}");
                    else
                        sb.AppendLine($"{f.label}: {f.value}");
                }
                facts.text = sb.ToString();
            }
        }
    }
}

