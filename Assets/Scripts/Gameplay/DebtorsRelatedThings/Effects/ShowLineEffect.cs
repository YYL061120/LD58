// Assets/Scripts/Gameplay/Effects/ShowLineEffect.cs
using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Show Line")]
    public class ShowLineEffect : EffectSO
    {
        public bool onLeft = true;      // 左=欠款人；右=玩家
        [TextArea] public string text;

        public override void Apply(CaseRuntime rt)
        {
            // 优先找新的 DialogueBubblesUI
            var ui = DialogueBubblesUI.Current ?? Object.FindFirstObjectByType<DialogueBubblesUI>();
            if (ui)
            {
                if (onLeft) ui.ShowLeft(text); else ui.ShowRight(text);
                return;
            }

            // 兼容老项目：若场景里还有 TalkUIHub 也支持
            var hub = Object.FindFirstObjectByType<TalkUIHub>();
            if (hub)
            {
                if (onLeft) hub.ShowLeft(text); else hub.ShowRight(text);
            }
        }

        public override string GetReadable() => $"显示({(onLeft ? "左" : "右")}): {text}";
    }
}
