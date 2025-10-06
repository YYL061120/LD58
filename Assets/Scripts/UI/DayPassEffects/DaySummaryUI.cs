// Assets/Scripts/UI/DaySummaryUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace DebtJam
{
    public class DaySummaryUI : MonoBehaviour
    {
        [Header("Refs")]
        public CanvasGroup root;        // 黑幕父物体（建议整个全屏 Panel 上挂 CanvasGroup）
        public TMP_Text summaryText;    // 文本
        public Button continueButton;   // “继续”按钮（可选）
        public TypewriterTMP typewriter; // TMP 的打字机（见下一个脚本）

        [Header("Fx")]
        public float fadeTime = 0.25f;

        void Awake()
        {
            HideImmediate();
            if (continueButton) continueButton.onClick.AddListener(OnContinueClicked);
        }

        void OnEnable()
        {
            if (GameClock.I != null)
            {
                GameClock.I.OnDayEnded += OnDayEnded;
                GameClock.I.OnGameEnded += OnGameEnded;
            }
        }

        void OnDisable()
        {
            if (GameClock.I != null)
            {
                GameClock.I.OnDayEnded -= OnDayEnded;
                GameClock.I.OnGameEnded -= OnGameEnded;
            }
        }

        void OnDayEnded(int day, int collectedToday, int daysLeft)
        {
            // 组合文案
            string msg =
                $"第 {day} 天结束\n" +
                $"今日追回：${collectedToday}\n" +
                $"还有 {daysLeft} 天老板就来检查";

            StartCoroutine(ShowRoutine(msg, waitForContinue: true));
        }

        void OnGameEnded()
        {
            string end = MoneyManager.I ? MoneyManager.I.GetEnding() : "——";
            int total = MoneyManager.I ? MoneyManager.I.totalCollected : 0;
            string msg =
                $"考核结束\n" +
                $"总计追回：${total}\n" +
                $"{end}\n\n" +
                $"（按任意键返回主界面/重开）";
            StartCoroutine(ShowRoutine(msg, waitForContinue: false));
        }

        IEnumerator ShowRoutine(string text, bool waitForContinue)
        {
            summaryText.text = "";
            Show();

            // 打字机
            if (typewriter) yield return typewriter.PlayCoroutine(summaryText, text);
            else summaryText.text = text;

            if (waitForContinue)
            {
                if (continueButton) continueButton.gameObject.SetActive(true);
                // 任意键也可以继续
                while (!Input.anyKeyDown) { yield return null; }
                OnContinueClicked();
            }
            else
            {
                // 结束面板保持，或在几秒后自动隐藏（按需要自行处理）
            }
        }

        void OnContinueClicked()
        {
            if (continueButton) continueButton.gameObject.SetActive(false);
            Hide();
            GameClock.I?.ConfirmNextDay();
        }

        void Show()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeTo(1f));
        }
        void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeTo(0f, () => gameObject.SetActive(false)));
        }
        void HideImmediate()
        {
            if (root) { root.alpha = 0f; root.blocksRaycasts = false; root.interactable = false; }
            gameObject.SetActive(false);
        }

        IEnumerator FadeTo(float target, System.Action onDone = null)
        {
            if (!root) yield break;
            root.gameObject.SetActive(true);
            root.blocksRaycasts = target > 0;
            root.interactable = target > 0;

            float start = root.alpha;
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                root.alpha = Mathf.Lerp(start, target, t / fadeTime);
                yield return null;
            }
            root.alpha = target;
            onDone?.Invoke();
        }
    }
}
