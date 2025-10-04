using System.Collections.Generic;

namespace DebtJam
{
    public class CaseRuntime
    {
        public string debtorId;
        public string displayName;

        // 事实（词条）
        public Dictionary<string, FactState> facts = new();

        // 联系方式
        public bool hasPhone; public string phoneNumber;
        public bool hasAddress; public string address;
        public bool hasEmail; public string email;

        // 运行状态
        public HashSet<string> milestones = new();
        public int goodwill = 0;
        public CaseOutcome outcome = CaseOutcome.Pending;

        // 行动历史（用于顺序条件）
        public List<ActionType> actionHistory = new();

        public CaseRuntime(DebtorProfileSO so)
        {
            debtorId = so.debtorId;
            displayName = so.displayName;

            foreach (var fd in so.initialFacts)
            {
                facts[fd.key] = new FactState
                {
                    key = fd.key,
                    label = fd.uiLabel,
                    value = fd.value,
                    visibility = fd.visibility
                };
            }

            hasPhone = so.hasPhoneAtStart; phoneNumber = so.phoneNumber;
            hasAddress = so.hasAddressAtStart; address = so.address;
            hasEmail = so.hasEmailAtStart; email = so.email;

            goodwill = so.startGoodwill;
            outcome = so.startOutcome;

            foreach (var m in so.startMilestones) milestones.Add(m);
        }
    }

    public class FactState
    {
        public string key;
        public string label;  // UI展示名
        public string value;
        public FactVisibility visibility;
    }
}
