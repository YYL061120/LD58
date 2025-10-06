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
        public CanvasGroup root;          // 黑幕父物体（挂 CanvasGroup）
        public TMP_Text summaryText;      // 文本
        public Button continueButton;     // “继续”按钮（可选）
        public TypewriterTMP typewriter;  // TMP 打字机（可选）

        [Header("Fx")]
        public float fadeTime = 0.25f;

        [Header("Behaviour")]
        public bool lockWorldWhenShown = true;   // 弹出时锁交互
        public bool anyKeyToContinue = true;     // 是否支持任意键继续（仅日结时）
        public float anyKeyDelay = 0.15f;        // 打字完毕后等这么久才接收任意键

        // —— 内部状态 —— //
        Coroutine _co;
        bool _isShowing;
        bool _awaitingContinue;

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

        // === 事件回调 ===
        void OnDayEnded(int day, int collectedToday, int daysLeft)
        {
            string msg =
                $"第 {day} 天结束\n" +
                $"今日追回：{FmtMoney(collectedToday)}\n" +
                $"还有 {daysLeft} 天老板就来检查";

            StartShow(msg, waitForContinue: true);
        }

        void OnGameEnded()
        {
            string end = MoneyManager.I ? MoneyManager.I.GetEnding() : "——";
            int totalAll = MoneyManager.I ? MoneyManager.I.totalCollected : 0;

            string msg =
                $"考核结束\n" +
                $"总计追回：{FmtMoney(totalAll)}\n" +
                $"{end}\n\n" +
                $"（按任意键返回主界面/重开）";

            // 最后一天：不主动关闭，由你自己的“回主界面/重开”流程来关这个面板
            StartShow(msg, waitForContinue: false);
        }

        // === 展示/隐藏 ===
        void StartShow(string text, bool waitForContinue)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(ShowRoutine(text, waitForContinue));
        }

        IEnumerator ShowRoutine(string text, bool waitForContinue)
        {
            _isShowing = true;
            _awaitingContinue = false;

            if (continueButton) continueButton.gameObject.SetActive(false);
            if (summaryText) summaryText.text = "";

            Show();
            if (lockWorldWhenShown) InteractableItemsController.I?.Lock("[DaySummaryUI] showing");

            // 打字机或瞬显
            if (typewriter) yield return typewriter.PlayCoroutine(summaryText, text);
            else if (summaryText) summaryText.text = text;

            if (waitForContinue)
            {
                _awaitingContinue = true;
                if (continueButton) continueButton.gameObject.SetActive(true);

                float t = 0f;
                // 防抖：给 UI 一点点时间再接受任意键
                while (anyKeyToContinue && t < anyKeyDelay)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                // 等待：任意键 或 点击按钮
                while (_awaitingContinue)
                {
                    if (anyKeyToContinue && Input.anyKeyDown)
                        break;
                    yield return null;
                }

                OnContinueClicked(); // 统一走这里
            }
            else
            {
                // 结局：保持显示，交互仍然锁住（让玩家看完/进入你的结局流程）
                // 如需自动隐藏，可自行在此处加 WaitForSecondsRealtime 然后 Hide();
            }
        }

        void OnContinueClicked()
        {
            if (!_isShowing) return;    // 防止重复触发
            _awaitingContinue = false;

            if (continueButton) continueButton.gameObject.SetActive(false);

            Hide();
            if (lockWorldWhenShown) InteractableItemsController.I?.Unlock("[DaySummaryUI] next day");

            GameClock.I?.ConfirmNextDay();
        }

        // === UI 控制 ===
        void Show()
        {
            gameObject.SetActive(true);
            if (_co != null) StopCoroutine(_co);
            StartCoroutine(FadeTo(1f));
        }

        void Hide()
        {
            if (_co != null) StopCoroutine(_co);
            StartCoroutine(FadeTo(0f, () =>
            {
                _isShowing = false;
                gameObject.SetActive(false);
            }));
        }

        void HideImmediate()
        {
            if (root)
            {
                root.alpha = 0f;
                root.blocksRaycasts = false;
                root.interactable = false;
            }
            _isShowing = false;
            gameObject.SetActive(false);
        }

        IEnumerator FadeTo(float target, System.Action onDone = null)
        {
            if (!root) { onDone?.Invoke(); yield break; }

            root.gameObject.SetActive(true);
            root.blocksRaycasts = target > 0f;
            root.interactable = target > 0f;

            float start = root.alpha;
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                root.alpha = Mathf.Lerp(start, target, t / fadeTime);
                yield return null;
            }
            root.alpha = target;
            onDone?.Invoke();
        }

        // === 小工具 ===
        static string FmtMoney(int v) => "¥" + v.ToString("#,0");
    }
}
