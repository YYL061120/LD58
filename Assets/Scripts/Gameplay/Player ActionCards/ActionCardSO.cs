// Assets/Scripts/Gameplay/Player ActionCards/ActionCardSO.cs
using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    /// <summary>
    /// 一张“行动卡”（打电话/短信/上门）的完整对话流程。
    /// 由多个 Step 组成：进入 Step → 执行 onEnterEffects →（可选）显示 NPC 台词 →（可选）给出玩家选项 → 根据选项跳下一步。
    /// </summary>
    [CreateAssetMenu(menuName = "DebtJam/Action Card")]
    public class ActionCardSO : ScriptableObject
    {
        [Header("步骤（按顺序配置）")]
        public List<ActionStep> steps = new();

        /// <summary>给当前案件选择“第一个可用 Step”。逐个检测 gate 条件，命中即返回；都不满足就回退到 steps[0]。</summary>
        public ActionStep GetStepFor(CaseRuntime rt)
        {
            if (steps == null || steps.Count == 0) return null;

            // 有 gate 条件且满足 → 作为入口
            foreach (var s in steps)
                if (s == null || s.GateMet(rt)) return s;

            return steps[0];
        }

        /// <summary>根据当前 Step 和玩家所选 Option 计算下一步。</summary>
        public ActionStep GetNextStep(ActionStep cur, ActionOption chosen, CaseRuntime rt)
        {
            // 选项指定了覆盖的下一步 → 用它
            if (chosen != null && chosen.nextStepOverride != null)
                return chosen.nextStepOverride;

            // 否则用 Step 自带的 nextStep
            if (cur != null && cur.nextStep != null)
                return cur.nextStep;

            // 都没配，尝试用“列表顺序的下一位”兜底
            if (cur != null && steps != null)
            {
                int idx = steps.IndexOf(cur);
                if (idx >= 0 && idx + 1 < steps.Count) return steps[idx + 1];
            }
            return null;
        }
    }

    // ===================== 数据结构 =====================

    [System.Serializable]
    public class ActionStep
    {
        [Header("作为入口 Step 的门槛（全部满足）")]
        public ConditionSO[] gateConditions;

        [Header("进入该 Step 时立即执行（不扣时间）")]
        public EffectSO[] onEnterEffects;

        [Header("进入该 Step 时展示的台词（谁先说、说几句都可交给 Effects 控制；这里是一个快捷位：NPC 左气泡）")]
        [TextArea] public string npcLine;

        [Header("此 Step 是否把“选项按钮”展示出来")]
        public bool showOptions = true;

        [Header("玩家可选项（可为空）")]
        public List<ActionOption> options = new();

        [Header("默认下一步（当选项未覆盖 nextStep 时使用）")]
        public ActionStep nextStep;

        public bool GateMet(CaseRuntime rt)
        {
            if (gateConditions == null || gateConditions.Length == 0) return true;
            foreach (var c in gateConditions)
            {
                if (c == null) continue;
                if (!c.Evaluate(rt, GameClock.I)) return false;
            }
            return true;
        }

        public void RunEnter(CaseRuntime rt)
        {
            if (onEnterEffects != null)
                foreach (var e in onEnterEffects)
                    if (e != null) e.Apply(rt);
        }
    }

    [System.Serializable]
    public class ActionOption
    {
        [Header("按钮文字 / 玩家台词（右气泡显示该文本；若空则用按钮文字）")]
        public string optionText;
        [TextArea] public string playerLineForUI;

        [Header("选项是否可见/可点（全部满足）")]
        public ConditionSO[] conditions;

        [Header("点击后执行的效果（Reveal/里程碑/加钱/DeadEnd/Collected 等）")]
        public EffectSO[] effects;

        [Header("对话控制")]
        public bool endsDialogue = false;         // 结束对话
        public bool triggerCollected = false;     // 点完即收款
        public ActionStep nextStepOverride;       // 覆盖默认下一步

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

        public void ApplyEffects(CaseRuntime rt)
        {
            if (effects == null) return;
            foreach (var e in effects)
                if (e != null) e.Apply(rt);

            CaseManager.I?.NotifyCaseChanged(rt);
        }

        public string GetPlayerLineForUI()
            => string.IsNullOrEmpty(playerLineForUI) ? optionText : playerLineForUI;
    }
}
