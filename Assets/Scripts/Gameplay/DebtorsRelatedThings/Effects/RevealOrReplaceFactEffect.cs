using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Reveal or Replace Fact")]
    public class RevealOrReplaceFactEffect : EffectSO
    {
        public string factKey;
        [TextArea] public string newValue;
        [Tooltip("true=标记为已知真，false=标记为已知假（仍会覆盖显示新值）")]
        public bool markTrue = true;

        public override void Apply(CaseRuntime rt)
        {
            if (rt == null || string.IsNullOrWhiteSpace(factKey)) return;

            // 用运行态封装：里头会自动做 <s>旧</s> 新，并把 Unknown 变成 KnownTrue
            rt.RevealOrReplaceFact(factKey, factKey, newValue);

            // 补：如果你想用“已知假”的语义，覆盖一下 state
            if (!markTrue && rt.facts.TryGetValue(factKey, out var f))
                f.state = FactState.Fake;

            CaseManager.I?.NotifyCaseChanged(rt); // 通知 UI 刷新
        }

        public override string GetReadable() =>
            $"Reveal/Replace '{factKey}' -> {(markTrue ? "True" : "Fake")}: {newValue}";
    }
}
