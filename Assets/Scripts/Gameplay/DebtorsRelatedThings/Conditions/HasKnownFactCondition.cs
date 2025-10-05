using UnityEngine;

namespace DebtJam
{
    /// <summary>
    /// 判断运行态中某条 fact 是否“已知”(KnownTrue)。
    /// requireTrue = true  → 必须是 KnownTrue
    /// requireTrue = false → 只要不是 Unknown（即 KnownTrue 或 Fake 都算“已知”）
    /// </summary>
    [CreateAssetMenu(menuName = "DebtJam/Conditions/Has Known Fact")]
    public class HasKnownFactCondition : ConditionSO
    {
        public string factKey;
        public bool requireTrue = true; // true=必须已知真；false=只要非 Unknown 即可

        public override bool Evaluate(CaseRuntime rt, GameClock clock)
        {
            if (rt == null || string.IsNullOrWhiteSpace(factKey)) return false;

            if (!rt.facts.TryGetValue(factKey, out var f))
                return false; // 根本没有这条 fact

            // 新运行态：用 f.state（FactState 枚举），不要再用 f.visibility
            return requireTrue
                ? f.state == FactState.KnownTrue
                : f.state != FactState.Unknown; // KnownTrue 或 Fake 都视为“已知”
        }

        public override string GetReadable() =>
            requireTrue ? $"已知真: {factKey}" : $"已知(非 Unknown): {factKey}";
    }
}
