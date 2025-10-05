using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Debtor Profile")]
    public class DebtorProfileSO : ScriptableObject
    {
        [Header("标识/显示")]
        public string debtorId;      // A/B/C/D/E/F
        public string displayName;
        public Sprite portrait;
        [TextArea] public string description;
        public int amountOwed;

        [Header("初始联系方式")]
        public bool hasPhoneAtStart; public string phoneNumber;
        public bool hasAddressAtStart; public string address;
        //public bool hasEmailAtStart;/* public string email;*/

        [Header("初始词条（Unknown=模糊；Fake=错误；True=正确）")]
        public List<FactDef> initialFacts = new();

        [Header("初始状态")]
        public CaseOutcome startOutcome = CaseOutcome.Pending;

        [Header("三种行动卡")]
        public ActionCardSO callCard;
        public ActionCardSO smsCard;
        public ActionCardSO visitCard;

        [Header("前置完成后才显示")]
        public List<string> visibleAfterCollectedTheseIds = new();
        // A：空；B/C：["A"]；D/E/F：["B"]
    }

    [System.Serializable]
    public class FactDef
    {
        public string key;     // "RealName","Address","PhoneOwner"…
        public string uiLabel; // UI标签
        public string value;
        public FactVisibility visibility = FactVisibility.Unknown;
    }
}
