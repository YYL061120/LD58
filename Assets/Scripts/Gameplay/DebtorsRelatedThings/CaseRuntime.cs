using System.Collections.Generic;
using System.Linq;

namespace DebtJam
{

    /// <summary>运行时事实</summary>
    public class FactRT
    {
        public string key;
        public string label;
        public string value;

        // 兼容：如果你的效果脚本用的是 FactState，就读这个
        public FactState state = FactState.KnownTrue;

        // 用于 UI 显示划线旧值：<s>old</s> new
        public string oldValueStriked;
    }

    /// <summary>
    /// 案件运行态：效果与条件都只改/读这里
    /// （补齐：milestones、actionHistory、smsLoopLocked、SetContact 等）
    /// </summary>
    public class CaseRuntime
    {
        // 基础身份 / 金额
        public string debtorId;
        public string displayName;
        public int amountOwed;

        // 联系方式（可被效果修改）
        public bool hasPhone;
        public string phoneNumber;
        public bool hasAddress;
        public string address;

        // 可见性 & 终局
        public bool isVisible;
        public CaseOutcome outcome = CaseOutcome.Pending;

        // 运行态事实（Profile 用）
        public readonly Dictionary<string, FactRT> facts = new();

        // === 兼容老效果/条件用到的字段 ===
        public readonly HashSet<string> milestones = new();   // Add/Has/Remove
        public readonly List<ActionType> actionHistory = new(); // 用于 LastActionWas 条件
        public bool smsLoopLocked = false;                    // 用于 LockSmsLoopEffect

        public CaseRuntime(DebtorProfileSO so, bool visibleAtStart)
        {
            debtorId    = so.debtorId;
            displayName = so.displayName;
            amountOwed  = so.amountOwed;

            hasPhone    = so.hasPhoneAtStart;
            phoneNumber = so.phoneNumber;
            hasAddress  = so.hasAddressAtStart;
            address     = so.address;

            isVisible   = visibleAtStart;

            facts.Clear();
            if (so.initialFacts != null)
            {
                foreach (var f in so.initialFacts)
                {
                    if (string.IsNullOrWhiteSpace(f.key)) continue;
                    facts[f.key] = new FactRT
                    {
                        key = f.key,
                        label = f.label,
                        value = f.value,
                        // 兼容：把 SO 里的可见性映射到 FactState
                        state = f.visibility == FactVisibility.Unknown ? FactState.Unknown : FactState.KnownTrue,
                        oldValueStriked = f.oldValueStriked
                    };
                }
            }
        }

        // ========== 给效果层调用的便捷方法 ==========

        /// <summary>记录一次行动（扣时逻辑在 ActionExecutor 里，这里只记轨迹）</summary>
        public void PushAction(ActionType type)
        {
            if (type == ActionType.None) return;
            actionHistory.Add(type);
            if (actionHistory.Count > 16) actionHistory.RemoveAt(0); // 防止无限增长
        }

        /// <summary>最近一次行动是否为 X（供 LastActionWasCondition 使用）</summary>
        public bool LastActionWas(ActionType type)
        {
            if (actionHistory.Count == 0) return false;
            return actionHistory[^1] == type;
        }

        /// <summary>添加/移除/判断里程碑（供 HasMilestoneCondition / AddOrRemoveMilestoneEffect）</summary>
        public void AddMilestone(string key)    { if (!string.IsNullOrEmpty(key)) milestones.Add(key); }
        public void RemoveMilestone(string key) { if (!string.IsNullOrEmpty(key)) milestones.Remove(key); }
        public bool HasMilestone(string key)    { return !string.IsNullOrEmpty(key) && milestones.Contains(key); }

        /// <summary>锁/解锁“短信循环”</summary>
        public void LockSmsLoop(bool on) => smsLoopLocked = on;

        /// <summary>修改联系方式（用于 SetContactEffect）</summary>
        public void SetContact(ContactKind kind, string newVal)
        {
            switch (kind)
            {
                case ContactKind.Phone:
                    phoneNumber = newVal;
                    hasPhone = !string.IsNullOrWhiteSpace(newVal);
                    break;
                case ContactKind.Address:
                    address = newVal;
                    hasAddress = !string.IsNullOrWhiteSpace(newVal);
                    break;
            }
        }

        /// <summary>揭示或替换事实。若已存在则做“<s>旧</s> 新”；若不存在则创建</summary>
        public void RevealOrReplaceFact(string key, string label, string newValue)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            if (!facts.TryGetValue(key, out var f))
            {
                f = new FactRT { key = key, label = label, value = newValue, state = FactState.KnownTrue };
                facts[key] = f;
                return;
            }

            // 记下旧值，为 UI 划线
            if (!string.IsNullOrEmpty(f.value) && f.value != newValue)
                f.oldValueStriked = f.value;

            f.value = newValue;
            f.state = FactState.KnownTrue; // 变为“已知”
        }
    }
}
