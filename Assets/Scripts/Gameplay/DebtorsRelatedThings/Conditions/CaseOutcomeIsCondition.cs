using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Case Outcome Is")]
    public class CaseOutcomeIsCondition : ConditionSO
    {
        public CaseOutcome expected;
        public override bool Evaluate(CaseRuntime rt, GameClock clock) => rt.outcome == expected;
        public override string GetReadable() => $"案件=={expected}";
    }
}
