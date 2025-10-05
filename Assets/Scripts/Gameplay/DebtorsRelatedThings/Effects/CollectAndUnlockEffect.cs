using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Collect And Unlock")]
    public class CollectAndUnlockEffect : EffectSO
    {
        public override void Apply(CaseRuntime rt)
        {
            if (rt.outcome == CaseOutcome.Collected) return;
            rt.outcome = CaseOutcome.Collected;
            MoneyManager.I?.Add(rt.amountOwed);
            CaseManager.I?.UnlockByProgress(); // A 完成解锁 B/C；B 完成解锁 D/E/F
        }
        public override string GetReadable() => "收回欠款并解锁后续";
    }
}


