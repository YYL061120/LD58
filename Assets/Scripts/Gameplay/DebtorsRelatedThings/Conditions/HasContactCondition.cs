using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Has Contact")]
    public class HasContactCondition : ConditionSO
    {
        public ContactKind type;
        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            return type switch
            {
                ContactKind.Phone => rt.hasPhone,
                ContactKind.Address => rt.hasAddress,
                //ContactType.Email => rt.hasEmail,
                _ => false
            };
        }
        public override string GetReadable() => $"有联系方式：{type}";
    }
}

