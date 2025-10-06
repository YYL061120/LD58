using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Blur Fact (Strike & Unknown)")]
    public class BlurFactEffect : EffectSO
    {
        [Tooltip("要模糊的 Fact Key（例如 Address/Phone 等）")]
        public string factKey;

        public override void Apply(CaseRuntime rt)
        {
            if (string.IsNullOrWhiteSpace(factKey)) return;
            if (!rt.facts.TryGetValue(factKey, out var f)) return;

            // 记录旧值到划线字段
            if (!string.IsNullOrEmpty(f.value))
                f.oldValueStriked = f.value;

            // 变为“未知”
            f.value = "";
            f.state = FactState.Unknown;

            CaseManager.I?.NotifyCaseChanged(rt); // 刷 UI
        }

        public override string GetReadable() => $"模糊 {factKey}";
    }
}
