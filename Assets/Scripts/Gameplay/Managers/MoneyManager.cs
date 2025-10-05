using UnityEngine;
using System;

namespace DebtJam
{
    public class MoneyManager : MonoBehaviour
    {
        public static MoneyManager I { get; private set; }

        public int totalCollected;
        public event Action<int> OnMoneyChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>直接加钱（非案件方式，例如奖励等）</summary>
        public void Add(int amount)
        {
            totalCollected += Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke(totalCollected);
        }

        /// <summary>根据案件一次性收款（推荐）。会把案件 outcome 改为 Collected 并锁定金额。</summary>
        public void Collect(CaseRuntime rt)
        {
            if (rt == null) return;

            // 已经收过则跳过
            if (rt.outcome == CaseOutcome.Collected) return;

            // 若是 DeadEnd 也不应收
            if (rt.outcome == CaseOutcome.DeadEnd) return;

            rt.outcome = CaseOutcome.Collected;
            Add(rt.amountOwed);

            // 通知 CaseManager 解锁后续可见角色
            CaseManager.I?.UnlockByProgress();
            CaseManager.I?.NotifyCaseChanged(rt);
        }

        /// <summary>按金额收款（不改案件状态）。除非你有特殊需要，一般用 Collect(CaseRuntime)。</summary>
        public void CollectAmount(int amount)
        {
            Add(amount);
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
