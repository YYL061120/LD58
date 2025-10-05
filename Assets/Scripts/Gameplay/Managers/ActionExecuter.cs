using UnityEngine;

namespace DebtJam
{
    public class ActionExecutor : MonoBehaviour
    {
        public static ActionExecutor I { get; private set; }
        void Awake() { if (I && I != this) { Destroy(gameObject); return; } I = this; DontDestroyOnLoad(gameObject); }

        // ✅ Call=120min, SMS=60min, Visit=180min
        int CostMinutes(ActionType t) => t == ActionType.Call ? 120 : (t == ActionType.SMS ? 60 : 180);

        public bool TryExecute(ActionCardSO card, ActionOption opt, string debtorId, out string fail)
        {
            fail = null;
            var cm = CaseManager.I; var clock = GameClock.I;

            if (!cm.runtimeById.TryGetValue(debtorId, out var rt)) { fail = "无此欠款人"; return false; }
            if (!rt.isVisible) { fail = "该档案尚未出现"; return false; }
            if (rt.outcome != CaseOutcome.Pending) { fail = "案件已终结"; return false; }

            int minutes = CostMinutes(card.actionType);
            if (!clock.CanAfford(minutes)) { fail = "今天工时不足"; return false; }

            // 逐个条件检查（AND）
            foreach (var c in opt.conditions)
            {
                if (c == null) continue;
                if (!c.Evaluate(rt, clock)) { fail = $"条件不满足：{c.GetReadable()}"; return false; }
            }

            // 扣时 + 记录历史
            clock.Spend(minutes);
            rt.actionHistory.Add(card.actionType);

            // 按顺序执行效果
            foreach (var fx in opt.effects) if (fx != null) fx.Apply(rt);

            // 刷新 UI
            //cm.OnCaseChanged?.Invoke(rt);
            cm.NotifyCaseChanged(rt);
            return true;
        }
    }
}

