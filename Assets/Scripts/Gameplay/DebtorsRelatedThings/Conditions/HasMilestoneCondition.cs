using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Has Milestone")]
    public class HasMilestoneCondition : ConditionSO
    {
        public string milestone;
        public bool requireAbsent = false; // 勾选=必须没有
        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            bool has = rt.milestones.Contains(milestone);
            return requireAbsent ? !has : has;
        }
        public override string GetReadable() => requireAbsent ? $"没有:{milestone}" : $"有:{milestone}";
    }
}

