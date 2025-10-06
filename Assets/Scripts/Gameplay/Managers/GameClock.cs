using UnityEngine;
using System;

namespace DebtJam
{
    public class GameClock : MonoBehaviour
    {
        public static GameClock I { get; private set; }

        [Header("Config")]
        public int totalDays = 5;
        public int hoursPerDay = 6;

        [Header("Default Action Costs (minutes)")]
        public int callMinutes = 120;
        public int smsMinutes = 60;
        public int visitMinutes = 180;

        [Header("Runtime (readonly)")]
        public int currentDay = 1;
        public int minutesLeftToday;
        public bool waitingNextDay = false;

        public event Action<int, int> OnTimeChanged;        // (day, minutesLeft)
        public event Action<int, int, int> OnDayEnded;       // (day, collectedToday, daysLeft)
        public event Action OnGameEnded;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            minutesLeftToday = hoursPerDay * 60;
            Debug.Log($"[GameClock] Start → day={currentDay} left={minutesLeftToday}");
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
        }

        public bool HasMinutesLeft(int minutes) => !waitingNextDay && minutesLeftToday >= minutes;

        public bool Consume(int minutes)
        {
            if (!HasMinutesLeft(minutes)) return false;
            Spend(minutes);
            return true;
        }

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

        public void Spend(int minutes)
        {
            if (waitingNextDay) return;

            minutesLeftToday = Mathf.Max(0, minutesLeftToday - Mathf.Max(0, minutes));
            Debug.Log($"[GameClock] Spend -{minutes} → day={currentDay} left={minutesLeftToday}");
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);

            if (minutesLeftToday <= 0) EndDay();
        }

        public void EndDay()
        {
            if (waitingNextDay) return;
            waitingNextDay = true;

            int collectedToday = MoneyManager.I ? MoneyManager.I.dailyCollected : 0;
            int daysLeft = Mathf.Max(0, totalDays - currentDay);
            Debug.Log($"[GameClock] DayEnded → day={currentDay} today=${collectedToday} daysLeft={daysLeft}");
            OnDayEnded?.Invoke(currentDay, collectedToday, daysLeft);
        }

        public void ConfirmNextDay()
        {
            if (!waitingNextDay) return;

            if (currentDay >= totalDays)
            {
                waitingNextDay = false;
                Debug.Log("[GameClock] GameEnded");
                OnGameEnded?.Invoke();
                return;
            }

            currentDay++;
            waitingNextDay = false;
            minutesLeftToday = hoursPerDay * 60;
            Debug.Log($"[GameClock] NextDay → day={currentDay} left={minutesLeftToday}");
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);

            MoneyManager.I?.StartNewDay();
        }
    }
}
