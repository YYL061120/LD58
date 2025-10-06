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
            var hub = Object.FindFirstObjectByType<TalkUIHub>();
            if (hub)
            {
                hub.QueueLine(onLeft, text);
                Debug.Log($"[ShowLineEffect] 入队 ({(onLeft ? "左" : "右")}): {text}");
                return;
            }

            // —— 兼容老 UI（若还在用 DialogueBubblesUI）——
            var ui = DialogueBubblesUI.Current ?? Object.FindFirstObjectByType<DialogueBubblesUI>();
            if (ui)
            {
                if (onLeft) ui.ShowLeft(text);
                else ui.ShowRight(text);
            }
        }

        public override string GetReadable() => $"显示({(onLeft ? "左" : "右")}): {text}";
    }
}
