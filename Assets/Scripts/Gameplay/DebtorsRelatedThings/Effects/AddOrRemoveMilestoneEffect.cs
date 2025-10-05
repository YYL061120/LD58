using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Add Milestone")]
    public class AddMilestoneEffect : EffectSO
    {
        public string milestone;
        public override void Apply(CaseRuntime rt) => rt.milestones.Add(milestone);
        public override string GetReadable() => $"里程碑+{milestone}";
    }

    [CreateAssetMenu(menuName = "DebtJam/Effects/Remove Milestone")]
    public class RemoveMilestoneEffect : EffectSO
    {
        public string milestone;
        public override void Apply(CaseRuntime rt) => rt.milestones.Remove(milestone);
        public override string GetReadable() => $"里程碑-{milestone}";
    }
}


