using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Reveal or Replace Fact")]
    public class RevealOrReplaceFactEffect : EffectSO
    {
        public string factKey;
        [TextArea] public string newValue;
        public bool markTrue = true; // false=标记为 KnownFake（少用）

        public override void Apply(CaseRuntime rt)
        {
            if (!rt.facts.TryGetValue(factKey, out var f))
            {
                f = new FactState { key = factKey, label = factKey, visibility = FactVisibility.Unknown };
                rt.facts[factKey] = f;
            }
            if (!string.IsNullOrEmpty(f.value) && f.value != newValue)
                f.oldValueStriked = f.value; // UI 会 <s>旧</s> 新

            f.value = newValue;
            f.visibility = markTrue ? FactVisibility.KnownTrue : FactVisibility.KnownFake;
        }
        public override string GetReadable() => $"{factKey} => {(markTrue ? "真" : "假")}:{newValue}";
    }
}

