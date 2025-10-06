using TMPro;
using UnityEngine;

namespace DebtJam
{
    public class TopRightHUD : MonoBehaviour
    {
        public TMP_Text totalMoneyText;
        public TMP_Text dayText;
        public TMP_Text timeLeftText;

        float _pullTimer;

        void Awake()
        {
            // 自动抓取（命名同你场景：Txt_TotalMoney / Txt_Day / Txt_TimeLeft）
            if (!totalMoneyText) totalMoneyText = transform.Find("Txt_TotalMoney")?.GetComponent<TMP_Text>();
            if (!dayText) dayText = transform.Find("Txt_Day")?.GetComponent<TMP_Text>();
            if (!timeLeftText) timeLeftText = transform.Find("Txt_TimeLeft")?.GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            if (MoneyManager.I != null)
            {
                MoneyManager.I.OnMoneyChanged += OnMoneyChanged;
                // 立即拉一次
                OnMoneyChanged(MoneyManager.I.totalCollected);
            }
            else Debug.LogWarning("[TopRightHUD] MoneyManager not found.");

            if (GameClock.I != null)
            {
                GameClock.I.OnTimeChanged += OnTimeChanged;
                OnTimeChanged(GameClock.I.currentDay, GameClock.I.minutesLeftToday);
            }
            else Debug.LogWarning("[TopRightHUD] GameClock not found.");

            _pullTimer = 0f;
        }

        void OnDisable()
        {
            if (MoneyManager.I != null) MoneyManager.I.OnMoneyChanged -= OnMoneyChanged;
            if (GameClock.I != null) GameClock.I.OnTimeChanged -= OnTimeChanged;
        }

        void Update()
        {
            // 兜底：每 0.5s 主动拉一次，防止你前期事件没连上
            _pullTimer += Time.unscaledDeltaTime;
            if (_pullTimer >= 0.5f)
            {
                _pullTimer = 0f;
                if (MoneyManager.I) OnMoneyChanged(MoneyManager.I.totalCollected);
                if (GameClock.I) OnTimeChanged(GameClock.I.currentDay, GameClock.I.minutesLeftToday);
            }
        }

        void OnMoneyChanged(int total)
        {
            if (totalMoneyText) totalMoneyText.text = $"${total:N0}";
            // Debug
            // Debug.Log($"[TopRightHUD] OnMoneyChanged → {total}");
        }

        void OnTimeChanged(int day, int minutesLeft)
        {
            if (dayText) dayText.text = $"Day {day}";
            if (timeLeftText)
            {
                int h = minutesLeft / 60;
                int m = minutesLeft % 60;
                timeLeftText.text = $"{h:00}:{m:00}";
            }
            // Debug
            // Debug.Log($"[TopRightHUD] OnTimeChanged → day={day}, left={minutesLeft}");
        }
    }
}
