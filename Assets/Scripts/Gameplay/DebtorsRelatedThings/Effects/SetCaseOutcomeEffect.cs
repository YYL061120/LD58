using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Set Case Outcome")]
    public class SetCaseOutcomeEffect : EffectSO
    {
        public CaseOutcome setTo;
        public override void Apply(CaseRuntime rt) => rt.outcome = setTo;
        public override string GetReadable() => $"案件=>{setTo}";
    }
}


