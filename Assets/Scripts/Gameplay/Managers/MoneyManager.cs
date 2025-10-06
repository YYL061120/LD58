using UnityEngine;
using System;

namespace DebtJam
{
    public class MoneyManager : MonoBehaviour
    {
        public static MoneyManager I { get; private set; }

        public int totalCollected;
        public int dailyCollected;

        // HUD 订阅这个事件
        public event Action<int> OnMoneyChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // 场景一开始就把当前值推给 HUD
            Debug.Log($"[MoneyManager] Start → push total={totalCollected}");
            OnMoneyChanged?.Invoke(totalCollected);
        }

        public void StartNewDay() => dailyCollected = 0;

        public void Add(int amount)
        {
            int add = Mathf.Max(0, amount);
            totalCollected += add;
            dailyCollected += add;
            Debug.Log($"[MoneyManager] Add +{add} → total={totalCollected}, daily={dailyCollected}");
            OnMoneyChanged?.Invoke(totalCollected);
        }

        public void Collect(CaseRuntime rt)
        {
            if (rt == null) return;
            if (rt.outcome == CaseOutcome.Collected) return;

            Add(rt.amountOwed);
            CaseManager.I?.SetOutcome(rt.debtorId, CaseOutcome.Collected);
        }

        public string GetEnding()
        {
            int m = totalCollected;
            if (m >= 50000) return "S 结局：破纪录";
            if (m >= 40000) return "A 结局：王牌";
            if (m >= 25000) return "B 结局：骨干";
            if (m >= 10000) return "C 结局：达标";
            return "D 结局：未达标";
        }
    }
}
