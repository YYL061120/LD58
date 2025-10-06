// Assets/Scripts/UI/TalkUIHub.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebtJam
{
    public class TalkUIHub : MonoBehaviour
    {
        public enum DialogueStyle { Bubbles, Chat }

        [Header("Avatars")]
        public Image leftAvatar;            // 欠债人头像
        public Image rightAvatar;           // 可选：玩家头像

        [Header("Bubbles (电话/上门)")]
        public TypewriterText leftBubble;
        public TypewriterText rightBubble;

        [Header("Chat (短信样式)")]
        public GameObject chatRoot;         // 滚动视图根
        public Transform chatContent;       // Content
        public GameObject msgLeftPrefab;    // 欠债人消息预制（含 TMP_Text/Text）
        public GameObject msgRightPrefab;   // 玩家消息预制

        [Header("Options")]
        public Transform optionRoot;
        public Button optionButtonPrefab;

        DialogueStyle _style = DialogueStyle.Bubbles;
        bool _isPhoneOrVisit;      // 电话/上门=逐字；短信=瞬显
        string _debtorId;
        ActionCardSO _card;
        ActionStep _currentStep;

        // ============ 外部入口（保留） ============
        public void OpenCall(string id, ActionCardSO card) { _style = DialogueStyle.Bubbles; _isPhoneOrVisit = true; Open(id, card); }
        public void OpenVisit(string id, ActionCardSO card) { _style = DialogueStyle.Bubbles; _isPhoneOrVisit = true; Open(id, card); }
        public void OpenSMS(string id, ActionCardSO card) { _style = DialogueStyle.Chat; _isPhoneOrVisit = false; Open(id, card); }

        /// <summary>供效果库 / 其他流程“临时插入一句话”时调用（左=欠债人）。</summary>
        public void ShowLeft(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (_style == DialogueStyle.Chat) AddChatLeft(text);
            else
            {
                if (_isPhoneOrVisit) leftBubble?.Play(text);
                else leftBubble?.Show(text);
            }
        }
        /// <summary>供效果库 / 其他流程“临时插入一句话”时调用（右=玩家）。</summary>
        public void ShowRight(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (_style == DialogueStyle.Chat) AddChatRight(text);
            else
            {
                if (_isPhoneOrVisit) rightBubble?.Play(text);
                else rightBubble?.Show(text);
            }
        }

        public void CloseSelf() => gameObject.SetActive(false);

        // ============ 内部流程（保持你现有流程） ============
        void Open(string debtorId, ActionCardSO card)
        {
            _debtorId = debtorId; _card = card;

            leftBubble?.Clear(); rightBubble?.Clear();
            ClearChat();

            // 样式切换
            if (chatRoot) chatRoot.SetActive(_style == DialogueStyle.Chat);
            if (leftBubble) leftBubble.gameObject.SetActive(_style == DialogueStyle.Bubbles);
            if (rightBubble) rightBubble.gameObject.SetActive(_style == DialogueStyle.Bubbles);

            // 头像
            if (CaseManager.I.GetSO(debtorId) is DebtorProfileSO so && leftAvatar)
                leftAvatar.sprite = so.portrait;

            // 入口 Step
            if (!CaseManager.I.runtimeById.TryGetValue(debtorId, out var rt)) return;
            _currentStep = _card?.GetStepFor(rt);
            ShowStep(_currentStep, rt);

            gameObject.SetActive(true);
        }

        void ShowStep(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();
            if (step == null) { gameObject.SetActive(false); return; }

            // 先执行 onEnterEffects（可由 ShowLineEffect 注入左/右台词）
            step.RunEnter(rt);

            // 再显示 NPC 快捷行（左）
            if (!string.IsNullOrEmpty(step.npcLine))
                ShowLeft(step.npcLine);

            if (step.showOptions) BuildOptions(step, rt);
        }

        void BuildOptions(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();
            if (step.options == null || step.options.Count == 0) return;

            foreach (var opt in step.options)
            {
                if (opt == null || !opt.ConditionsMet(rt)) continue;

                var btn = Instantiate(optionButtonPrefab, optionRoot);
                var tmp = btn.GetComponentInChildren<TMP_Text>(true);
                var txt = btn.GetComponentInChildren<Text>(true);
                (tmp ? (Object)tmp : txt).SetText(opt.optionText);

                btn.onClick.AddListener(() =>
                {
                    // 玩家台词（右）
                    var line = opt.GetPlayerLineForUI();
                    ShowRight(line);

                    // 执行效果（不扣时间）
                    opt.ApplyEffects(rt);
                    if (opt.triggerCollected) MoneyManager.I?.Collect(rt);

                    if (opt.endsDialogue)
                    {
                        gameObject.SetActive(false);
                        return;
                    }

                    // 下一步：option.nextStep 优先，其次 step.nextStep
                    _currentStep = _card.GetNextStep(step, opt, rt);
                    ShowStep(_currentStep, rt);
                });
            }
        }

        void ClearOptions()
        {
            if (!optionRoot) return;
            for (int i = optionRoot.childCount - 1; i >= 0; --i)
                Destroy(optionRoot.GetChild(i).gameObject);
        }

        // —— Chat 工具 —— 
        void ClearChat()
        {
            if (!chatContent) return;
            for (int i = chatContent.childCount - 1; i >= 0; --i)
                Destroy(chatContent.GetChild(i).gameObject);
        }
        void AddChatLeft(string s)
        {
            if (!msgLeftPrefab || !chatContent) { leftBubble?.Show(s); return; }
            var go = Instantiate(msgLeftPrefab, chatContent);
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            var txt = go.GetComponentInChildren<Text>(true);
            (tmp ? (Object)tmp : txt).SetText(s);
            ScrollToBottom();
        }
        void AddChatRight(string s)
        {
            if (!msgRightPrefab || !chatContent) { rightBubble?.Show(s); return; }
            var go = Instantiate(msgRightPrefab, chatContent);
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            var txt = go.GetComponentInChildren<Text>(true);
            (tmp ? (Object)tmp : txt).SetText(s);
            ScrollToBottom();
        }
        void ScrollToBottom()
        {
            var sr = chatRoot ? chatRoot.GetComponentInChildren<ScrollRect>() : null;
            if (sr) sr.verticalNormalizedPosition = 0f;
        }
    }

    // 小工具：TMP 或 UGUI Text 二选一赋值
    static class TextExt
    {
        public static void SetText(this Object maybeText, string s)
        {
            if (maybeText is TMP_Text tmp) tmp.text = s;
            else if (maybeText is Text ui) ui.text = s;
        }
    }
}
