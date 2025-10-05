using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Last Action Was")]
    public class LastActionWasCondition : ConditionSO
    {
        public ActionType requiredType;
        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            if (rt.actionHistory.Count == 0) return false;
            return rt.actionHistory[^1] == requiredType;
        }
        public override string GetReadable() => $"上一步={requiredType}";
    }
}
