// Assets/Scripts/Gameplay/Managers/ActionExecutor.cs
using UnityEngine;

namespace DebtJam
{
    public class ActionExecutor : MonoBehaviour
    {
        public static ActionExecutor I { get; private set; }

        [Header("Refs")]
        public GameClock clock;                    // 计时器（已有）
        public DialogueBubblesUI bubblesUI;        // 新增：气泡对话 UI
        public InteractableItemsController items;  // 为了关面板、禁用 Collider（可选）

        [Header("Costs (minutes)")]
        public int callCost = 120; // 2h
        public int smsCost = 60;  // 1h
        public int visitCost = 180; // 3h

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
            if (!clock) clock = Object.FindFirstObjectByType<GameClock>();
            if (!bubblesUI) bubblesUI = FindFirstObjectByType<DialogueBubblesUI>();
            if (!items) items = Object.FindFirstObjectByType<InteractableItemsController>();
        }

        public bool CanDo(ActionType type, CaseRuntime rt, out string reason)
        {
            reason = null;
            if (rt == null) { reason = "无效角色"; return false; }

            switch (type)
            {
                case ActionType.Call:
                case ActionType.SMS:
                    if (!rt.hasPhone || string.IsNullOrWhiteSpace(rt.phoneNumber))
                    { reason = "缺少电话号码"; return false; }
                    break;
                case ActionType.Visit:
                    if (!rt.hasAddress || string.IsNullOrWhiteSpace(rt.address))
                    { reason = "缺少地址"; return false; }
                    break;
            }
            int cost = GetCost(type);
            if (!clock.HasMinutesLeft(cost))
            { reason = "今天可用时间不足"; return false; }
            return true;
        }

        public int GetCost(ActionType type)
        {
            return type switch
            {
                ActionType.Call => callCost,
                ActionType.SMS => smsCost,
                ActionType.Visit => visitCost,
                _ => 0
            };
        }

        public TalkUIHub talkUI;   // 在 Inspector 绑定 TalkUIHub

        // Assets/Scripts/Gameplay/Managers/ActionExecutor.cs
        // ...
        public bool TryStartAction(ActionType type, CaseRuntime rt, System.Action closePanel, System.Action<string> showToast = null)
        {
            if (!CanDo(type, rt, out var reason))
            { showToast?.Invoke($"无法执行：{reason}"); return false; }

            if (!clock.Consume(GetCost(type)))
            { showToast?.Invoke("今天可用时间不足"); return false; }

            // 关掉当前面板
            closePanel?.Invoke();

            // 🔒 禁用所有交互
            InteractableItemsController.I?.Lock("dialogue");

            var so = CaseManager.I.GetSO(rt.debtorId);
            var card = type switch
            {
                ActionType.Call => so.callCard,
                ActionType.SMS => so.smsCard,
                ActionType.Visit => so.visitCard,
                _ => null
            };

            if (talkUI)
            {
                // 对话结束时解锁：见下一个小节
                talkUI.OnClosed -= OnDialogueClosed;
                talkUI.OnClosed += OnDialogueClosed;

                if (type == ActionType.Call) talkUI.OpenCall(rt.debtorId, card);
                else if (type == ActionType.SMS) talkUI.OpenSMS(rt.debtorId, card);
                else if (type == ActionType.Visit) talkUI.OpenVisit(rt.debtorId, card);
            }

            rt.PushAction(type);
            return true;

            void OnDialogueClosed()
            {
                talkUI.OnClosed -= OnDialogueClosed;
                InteractableItemsController.I?.Unlock("dialogue");
            }
        }


        /// <summary>TalkUIHub 点击选项时调用（不再扣时间）。</summary>
        public bool TryExecute(ActionCardSO card, ActionOption opt, string debtorId, out string failReason)
        {
            failReason = null;
            if (opt == null || card == null) { failReason = "无效选项"; return false; }
            if (!CaseManager.I.runtimeById.TryGetValue(debtorId, out var rt)) { failReason = "未找到角色"; return false; }

            if (!opt.ConditionsMet(rt)) { failReason = "条件不满足"; return false; }

            opt.ApplyEffects(rt);

            if (opt.triggerCollected)
                MoneyManager.I?.Collect(rt);

            CaseManager.I?.NotifyCaseChanged(rt);
            return true;
        }
    }
}
