using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Action Card")]
    public class ActionCardSO : ScriptableObject
    {
        public ActionType actionType;
        [Tooltip("此行动消耗的分钟数 Call: 20, SMS: 10, Visit: 60?")]
        public int costMinutes = 20;

        [TextArea] public string description;

        public List<ActionOption> options = new();
    }

    [System.Serializable]
    public class ActionOption
    {
        [TextArea] public string optionText;

        [Tooltip("全部满足才可执行")]
        public List<ConditionSO> conditions = new();

        [Tooltip("执行后的效果（按顺序应用）")]
        public List<EffectSO> effects = new();
    }
}
