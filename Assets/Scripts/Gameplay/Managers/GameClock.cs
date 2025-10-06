// Assets/Scripts/Gameplay/Managers/GameClock.cs
using UnityEngine;
using System;

namespace DebtJam
{
    public class GameClock : MonoBehaviour
    {
        public static GameClock I { get; private set; }

        [Header("Config")]
        public int totalDays = 5;      // 5 天
        public int hoursPerDay = 6;    // 每天 6 小时

        [Header("Default Action Costs (minutes)")]
        // 仅供 Consume(ActionType) 使用；你的 ActionExecutor 仍可用自己的 cost
        public int callMinutes = 120;   // 打电话 2h
        public int smsMinutes = 60;    // 发短信 1h
        public int visitMinutes = 180;   // 上门 3h

        [Header("Runtime (readonly)")]
        public int currentDay = 1;
        public int minutesLeftToday;
        public bool waitingNextDay = false; // 日结黑幕弹出后，等待 UI 确认

        // (dayJustEnded, collectedToday, daysLeft)
        public event Action<int, int, int> OnDayEnded;
        public event Action OnGameEnded;
        public event Action<int, int> OnTimeChanged; // (day, minutesLeft)

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            minutesLeftToday = hoursPerDay * 60;
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
        }

        // ======== 你原先在 ActionExecutor 里用到的两个接口：完全兼容 ========

        /// <summary>检查今天是否还剩足够的分钟（别名，给 ActionExecutor 调）。</summary>
        public bool HasMinutesLeft(int minutes) => !waitingNextDay && minutesLeftToday >= minutes;

        /// <summary>按“分钟数”扣时（别名，给 ActionExecutor 调）。成功返回 true。</summary>
        public bool Consume(int minutes)
        {
            if (!HasMinutesLeft(minutes)) return false;
            Spend(minutes);
            return true;
        }

        // ======== 之前版本里提供的接口仍然保留 ========

        public bool CanAfford(int minutes) => HasMinutesLeft(minutes);

        /// <summary>按“行动类型”扣时；若用你自己的 cost，可继续走 ActionExecutor 的 GetCost。</summary>
        public bool Consume(ActionType type)
        {
            int cost = type switch
            {
                ActionType.Call => callMinutes,
                ActionType.SMS => smsMinutes,
                ActionType.Visit => visitMinutes,
                _ => 0
            };
            return Consume(cost);
        }

        /// <summary>真正扣分钟 & 触发“用尽 → 日结”。</summary>
        public void Spend(int minutes)
        {
            if (waitingNextDay) return;

            minutesLeftToday = Mathf.Max(0, minutesLeftToday - Mathf.Max(0, minutes));
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);

            if (minutesLeftToday <= 0)
                EndDay();
        }

        /// <summary>今天用完：只广播“日结”，等待 UI 调 ConfirmNextDay()</summary>
        public void EndDay()
        {
            if (waitingNextDay) return;
            waitingNextDay = true;

            int collectedToday = MoneyManager.I ? MoneyManager.I.dailyCollected : 0;
            int daysLeft = Mathf.Max(0, totalDays - currentDay);
            OnDayEnded?.Invoke(currentDay, collectedToday, daysLeft);
        }

        /// <summary>日结黑幕点“继续”后进入下一天；最后一天则触发结束。</summary>
        public void ConfirmNextDay()
        {
            if (!waitingNextDay) return;

            if (currentDay >= totalDays)
            {
                waitingNextDay = false;
                OnGameEnded?.Invoke();
                return;
            }

            currentDay++;
            waitingNextDay = false;
            minutesLeftToday = hoursPerDay * 60;
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);

            MoneyManager.I?.StartNewDay();
        }
    }
}
