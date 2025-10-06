using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Action Card")]
    public class ActionCardSO : ScriptableObject
    {
        [Tooltip("卡片名，仅编辑用")]
        public string cardId;

        [Tooltip("进入卡片时可先触发的效果（不扣时间）")]
        public List<EffectSO> onEnterEffects = new();

        [Tooltip("对话步骤表")]
        public List<ActionStep> steps = new();

        [Tooltip("入口 Step Id（留空则用列表第一个）")]
        public string entryStepId;

        // —— 运行期查询：根据 CaseRuntime 决定“真正入口”
        public ActionStep GetStepFor(CaseRuntime rt)
        {
            // 先选 entryStepId；没有就第一个
            var step = string.IsNullOrWhiteSpace(entryStepId) ? steps.FirstOrDefault()
                                                              : steps.FirstOrDefault(s => s.stepId == entryStepId);
            return step;
        }

        // 用于从当前 Step 和玩家所选 Option 决定下一步
        public ActionStep GetNextStep(ActionStep current, ActionOption chosen, CaseRuntime rt)
        {
            if (chosen == null || string.IsNullOrEmpty(chosen.nextStepId)) return null;
            return steps.FirstOrDefault(s => s.stepId == chosen.nextStepId);
        }
    }

    [System.Serializable]
    public class ActionStep
    {
        [Tooltip("唯一 Id")]
        public string stepId;

        [Header("可选：进入本 Step 前的“守门条件”")]
        [Tooltip("全部满足才允许进入，否则走“被挡台词”")]
        public ConditionSO[] gateConditions;
        [Tooltip("守门条件不满足时的左侧台词（例如：对方抵触/门卫说已搬走）")]
        [TextArea] public string gateBlockedNpcLine;
        [Tooltip("被挡后是否立刻结束对话（true=结束；false=虽然被挡但仍展示选项）")]
        public bool gateEndsDialogue = true;

        [Header("正式台词（通过守门后才会播放）")]
        [TextArea] public string npcLine;

        [Header("进入本 Step 即刻生效的效果（不扣时间）")]
        public EffectSO[] onEnterEffects;

        [Header("显示选项")]
        public bool showOptions = true;
        public List<ActionOption> options = new();

        // 供 TalkUIHub 调用
        public void RunEnter(CaseRuntime rt)
        {
            if (onEnterEffects == null) return;
            foreach (var fx in onEnterEffects) if (fx != null) fx.Apply(rt);
        }

        // 守门检测
        public bool GatePassed(CaseRuntime rt)
        {
            if (gateConditions == null || gateConditions.Length == 0) return true;
            foreach (var c in gateConditions)
            {
                if (c == null) continue;
                if (!c.Evaluate(rt, GameClock.I)) return false;
            }
            return true;
        }
    }

    [System.Serializable]
    public class ActionOption
    {
        [Tooltip("按钮文案")]
        public string optionText;

        [Header("玩家台词")]
        [Tooltip("默认显示给 UI 的玩家台词")]
        public string playerLineForUI;

        [Header("抽象条件（全部满足才可点）")]
        public ConditionSO[] conditions;

        [Header("点击后效果（不再扣时间）")]
        public EffectSO[] effects;

        [Header("流转控制")]
        [Tooltip("true=结束对话")]
        public bool endsDialogue;
        [Tooltip("true=本次点击算收款（会进 MoneyManager、CaseManager）")]
        public bool triggerCollected;
        [Tooltip("下一步 stepId（留空则结束）")]
        public string nextStepId;

        public bool ConditionsMet(CaseRuntime rt)
        {
            if (conditions == null || conditions.Length == 0) return true;
            foreach (var c in conditions)
            {
                if (c == null) continue;
                if (!c.Evaluate(rt, GameClock.I)) return false;
            }
            return true;
        }

        public string GetPlayerLineForUI() => playerLineForUI;

        public void ApplyEffects(CaseRuntime rt)
        {
            if (effects == null) return;
            foreach (var fx in effects) if (fx != null) fx.Apply(rt);
        }
    }
}
