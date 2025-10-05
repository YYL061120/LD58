using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Show Line")]
    public class ShowLineEffect : EffectSO
    {
        public bool onLeft = true; // 左=欠款人；右=玩家
        [TextArea] public string text;

        public override void Apply(CaseRuntime rt)
        {
            var hub = Object.FindFirstObjectByType<TalkUIHub>();
            if (!hub) return;
            if (onLeft) hub.ShowLeft(text);
            else hub.ShowRight(text);
        }
        public override string GetReadable() => $"显示({(onLeft ? "左" : "右")}): {text}";
    }
}

