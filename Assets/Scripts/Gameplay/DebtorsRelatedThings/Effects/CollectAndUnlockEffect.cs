using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Collect And Unlock")]
    public class CollectAndUnlockEffect : EffectSO
    {
        [Tooltip("结案时是否加钱（使用 Debtor 的 amountOwed）。")]
        public bool alsoAddMoney = true;

        public override void Apply(CaseRuntime rt)
        {
            if (rt == null) return;
            if (rt.outcome == CaseOutcome.Collected) return;

            // 1) 加钱（可选）
            if (alsoAddMoney)
                MoneyManager.I?.Add(rt.amountOwed);

            // 2) 正式设置结局（→ 内部将负责解锁、刷新 UI、切换当前案件等）
            CaseManager.I?.SetOutcome(rt.debtorId, CaseOutcome.Collected);

            // 不在这里直接关 UI，由对话流程（endsDialogue）决定更自然
        }

        public override string GetReadable() => "收回欠款并解锁后续（并从 UI 列表移除）";
    }
}
