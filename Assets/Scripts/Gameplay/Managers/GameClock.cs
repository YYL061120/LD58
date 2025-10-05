using UnityEngine;
using System;

namespace DebtJam
{
    public class GameClock : MonoBehaviour
    {
        public static GameClock I { get; private set; }

        [Header("Config")]
        public int totalDays = 5;     // ✅ 五天
        public int hoursPerDay = 6;   // ✅ 每天六小时

        [Header("Runtime (readonly)")]
        public int currentDay = 1;
        public int minutesLeftToday;

        public event Action OnDayEnded;
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

        public bool CanAfford(int minutes) => minutesLeftToday >= minutes;

        public void Spend(int minutes)
        {
            minutesLeftToday = Mathf.Max(0, minutesLeftToday - minutes);
            OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
            if (minutesLeftToday <= 0) EndDay();
        }

        public void EndDay()
        {
            OnDayEnded?.Invoke();
            if (currentDay >= totalDays) OnGameEnded?.Invoke();
            else
            {
                currentDay++;
                minutesLeftToday = hoursPerDay * 60;
                OnTimeChanged?.Invoke(currentDay, minutesLeftToday);
            }
        }
    }
}