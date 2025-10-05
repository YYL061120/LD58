using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Action Card")]
    public class ActionCardSO : ScriptableObject
    {
        public ActionType actionType;         // Call / SMS / Visit
        [TextArea] public string description; // 策划备注
        public List<EffectSO> onEnterEffects = new(); // ✅ 新增：进入面板时立即播放的效果（不扣时间）
        public List<ActionOption> options = new();
    }

    [System.Serializable]
    public class ActionOption
    {
        [TextArea] public string optionText;      // 玩家在 UI 里看到的按钮文字
        public List<ConditionSO> conditions = new(); // 全部满足（AND）
        public List<EffectSO> effects = new();       // 按顺序执行
    }
}
