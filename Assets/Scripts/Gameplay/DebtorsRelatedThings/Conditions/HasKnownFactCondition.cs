using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Has Known Fact")]
    public class HasKnownFactCondition : ConditionSO
    {
        public string factKey;
        public bool requireTrue = true; // true=必须 KnownTrue；false=只要已揭示即可
        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            if (!rt.facts.TryGetValue(factKey, out var f)) return false;
            return requireTrue ? f.visibility == FactVisibility.KnownTrue
                               : f.visibility != FactVisibility.Unknown;
        }
        public override string GetReadable() => requireTrue ? $"已知真·{factKey}" : $"已知·{factKey}";
    }
}
