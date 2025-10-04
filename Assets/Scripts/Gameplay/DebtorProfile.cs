using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Debtor Profile")]
    public class DebtorProfileSO : ScriptableObject
    {
        public string debtorId;            // 唯一 ID
        public string displayName;
        public Sprite portrait;

        [Header("初始词条（可含假信息或未知）")]
        public List<FactDef> initialFacts = new();

        [Header("初始联系方式")]
        public bool hasPhoneAtStart;
        public string phoneNumber;
        public bool hasAddressAtStart;
        public string address;
        public bool hasEmailAtStart;
        public string email;

        [Header("初始状态")]
        public int startGoodwill = 0;
        public List<string> startMilestones = new();
        public CaseOutcome startOutcome = CaseOutcome.Pending;

        [Header("默认行动卡（按类型）")]
        public ActionCardSO callCard;
        public ActionCardSO smsCard;
        public ActionCardSO visitCard;
    }

    [System.Serializable]
    public class FactDef
    {
        public string key;         // e.g. "RealName", "Company", "Relation", "AltPhoneOwner"
        public string value;       // 展示文本
        public FactVisibility visibility = FactVisibility.Unknown; // Unknown / KnownTrue / KnownFake
        public string uiLabel;     // UI 显示名（如“公司”、“亲属”）
    }
}