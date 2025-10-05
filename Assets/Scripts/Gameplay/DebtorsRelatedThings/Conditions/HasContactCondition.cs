using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Has Contact")]
    public class HasContactCondition : ConditionSO
    {
        public ContactType type;
        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            return type switch
            {
                ContactType.Phone => rt.hasPhone,
                ContactType.Address => rt.hasAddress,
                //ContactType.Email => rt.hasEmail,
                _ => false
            };
        }
        public override string GetReadable() => $"有联系方式：{type}";
    }
}

