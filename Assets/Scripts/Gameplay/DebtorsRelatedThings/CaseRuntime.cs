using System.Collections.Generic;

namespace DebtJam
{
    public class CaseRuntime
    {
        public string debtorId, displayName;
        public int amountOwed;

        // 词条
        public Dictionary<string, FactState> facts = new();

        // 联系方式
        public bool hasPhone; public string phoneNumber;
        public bool hasAddress; public string address;
        public bool hasEmail; public string email;

        // 状态
        public HashSet<string> milestones = new();     // “短信抵抗”等
        public CaseOutcome outcome = CaseOutcome.Pending;
        public bool isVisible = false;                 // 是否出现在档案列表

        // 行动历史（用于 LastActionWas）
        public List<ActionType> actionHistory = new();

        public CaseRuntime(DebtorProfileSO so, bool visible)
        {
            debtorId = so.debtorId;
            displayName = so.displayName;
            amountOwed = so.amountOwed;

            foreach (var f in so.initialFacts)
                facts[f.key] = new FactState { key = f.key, label = f.uiLabel, value = f.value, visibility = f.visibility };

            hasPhone = so.hasPhoneAtStart; phoneNumber = so.phoneNumber;
            hasAddress = so.hasAddressAtStart; address = so.address;
            hasEmail = so.hasEmailAtStart; email = so.email;

            outcome = so.startOutcome;
            isVisible = visible;
        }
    }

    public class FactState
    {
        public string key, label, value;
        public FactVisibility visibility;
        public string oldValueStriked; // 被纠正时记录旧值，供 UI <s>旧</s> 新
    }
}

