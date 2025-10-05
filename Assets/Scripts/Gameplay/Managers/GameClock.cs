using UnityEngine;
using System;

namespace DebtJam
{
    /// <summary>
    /// 5 天、每天 6 小时的“行动时间”时钟。
    /// 提供：HasMinutesLeft / Consume / EndDay 以及事件回调。
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock I { get; private set; }

        [Header("Config")]
        [Tooltip("游戏总天数")]
        public int totalDays = 5;        // ✅ 五天
        [Tooltip("每天可用小时数")]
        public int hoursPerDay = 6;      // ✅ 每天六小时

        [Header("Runtime (readonly)")]
        [Tooltip("当前天（从 1 开始）")]
        public int currentDay = 1;
        [Tooltip("当日剩余分钟数")]
        public int minutesLeftToday;

        public event Action OnDayEnded;
        public event Action OnGameEnded;
        /// <summary>(day, minutesLeft)</summary>
        public event Action<int, int> OnTimeChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            ResetDayRuntime();
        }

        /// <summary>是否还有足够分钟可用（语义同 CanAfford）。</summary>
        public bool HasMinutesLeft(int minutes)
        {
            return minutesLeftToday >= Mathf.Max(0, minutes);
        }

        /// <summary>兼容旧接口。</summary>
        public bool CanAfford(int minutes) => HasMinutesLeft(minutes);

        /// <summary>
        /// 扣减分钟。成功返回 true；若不足则返回 false（不扣）。
        /// 扣到 0 会自动触发 EndDay()。
        /// </summary>
        public bool Consume(int minutes)
        {
            minutes = Mathf.Max(0, minutes);
            if (!HasMinutesLeft(minutes)) return false;

            minutesLeftToday -= minutes;
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);

            if (minutesLeftToday <= 0)
                EndDay();

            return true;
        }

        /// <summary>
        /// 兼容旧接口：等价于 Consume(minutes)；建议改用 Consume。
        /// </summary>
        [Obsolete("Use Consume(int minutes) instead.")]
        public void Spend(int minutes)
        {
            Consume(minutes);
        }

        /// <summary>可选：退款/补时（正数为加分钟，负数为减分钟）。不会跨天。</summary>
        public void AddMinutes(int deltaMinutes)
        {
            int before = minutesLeftToday;
            minutesLeftToday = Mathf.Clamp(before + deltaMinutes, 0, hoursPerDay * 60);
            if (minutesLeftToday != before)
                OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
        }

        /// <summary>手动结束当天；若到最后一天则触发 OnGameEnded。</summary>
        public void EndDay()
        {
            OnDayEnded?.Invoke();

            if (currentDay >= totalDays)
            {
                OnGameEnded?.Invoke();
            }
            else
            {
                currentDay++;
                ResetDayRuntime();
            }
        }

        /// <summary>重置当日分钟并广播一次时间变更。</summary>
        void ResetDayRuntime()
        {
            minutesLeftToday = Mathf.Max(0, hoursPerDay) * 60;
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
        }
    }
}
