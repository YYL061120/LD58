//// Assets/Scripts/UI/TalkUIHub.cs
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//namespace DebtJam
//{
//    public class TalkUIHub : MonoBehaviour
//    {
//        [Header("Avatars")]
//        public Image leftAvatar;
//        public Image rightAvatar;

//        [Header("Bubbles")]
//        public TypewriterText leftBubble;
//        public TypewriterText rightBubble;

//        [Header("Options")]
//        public Transform optionRoot;
//        public Button optionButtonPrefab;

//        string _debtorId;
//        ActionCardSO _card;
//        ActionStep _currentStep;
//        bool _isPhoneOrVisit; // 电话/上门=打字机；短信=瞬显

//        public void OpenCall(string id, ActionCardSO card) { _isPhoneOrVisit = true; Open(id, card); }
//        public void OpenVisit(string id, ActionCardSO card) { _isPhoneOrVisit = true; Open(id, card); }
//        public void OpenSMS(string id, ActionCardSO card) { _isPhoneOrVisit = false; Open(id, card); }

//        void Open(string debtorId, ActionCardSO card)
//        {
//            _debtorId = debtorId; _card = card;

//            leftBubble.Clear(); rightBubble.Clear();
//            if (CaseManager.I.GetSO(debtorId) is DebtorProfileSO so && leftAvatar) leftAvatar.sprite = so.portrait;

//            // 选择入口 Step
//            if (!CaseManager.I.runtimeById.TryGetValue(debtorId, out var rt)) return;
//            _currentStep = _card?.GetStepFor(rt);
//            ShowStep(_currentStep, rt);

//            gameObject.SetActive(true);
//        }

//        void ShowStep(ActionStep step, CaseRuntime rt)
//        {
//            ClearOptions();
//            if (step == null) { gameObject.SetActive(false); return; }

//            // 进入 Step：先效果，再台词
//            step.RunEnter(rt);
//            if (!string.IsNullOrEmpty(step.npcLine))
//            {
//                if (_isPhoneOrVisit) leftBubble.Play(step.npcLine);
//                else leftBubble.Show(step.npcLine);
//            }

//            if (step.showOptions) BuildOptions(step, rt);
//        }

//        void BuildOptions(ActionStep step, CaseRuntime rt)
//        {
//            ClearOptions();
//            if (step.options == null || step.options.Count == 0) return;

//            foreach (var opt in step.options)
//            {
//                if (opt == null) continue;
//                if (!opt.ConditionsMet(rt)) continue;

//                var btn = Instantiate(optionButtonPrefab, optionRoot);
//                var tmp = btn.GetComponentInChildren<TMP_Text>(true);
//                var txt = btn.GetComponentInChildren<Text>(true);
//                (tmp ? (Object)tmp : txt).SetText(opt.optionText);  // 小技巧：TMP/Text 二选一

//                btn.onClick.AddListener(() =>
//                {
//                    // 玩家台词（右）
//                    var line = opt.GetPlayerLineForUI();
//                    if (_isPhoneOrVisit) rightBubble.Play(line);
//                    else rightBubble.Show(line);

//                    // 执行效果（不扣时间）
//                    opt.ApplyEffects(rt);
//                    if (opt.triggerCollected) MoneyManager.I?.Collect(rt);

//                    if (opt.endsDialogue)
//                    {
//                        gameObject.SetActive(false);
//                        return;
//                    }

//                    // 进入下一 Step
//                    _currentStep = _card.GetNextStep(step, opt, rt);
//                    ShowStep(_currentStep, rt);
//                });
//            }
//        }

//        void ClearOptions()
//        {
//            foreach (Transform t in optionRoot) Destroy(t.gameObject);
//        }
//    }

//    static class TextExt
//    {
//        public static void SetText(this Object maybeText, string s)
//        {
//            if (maybeText is TMP_Text tmp) tmp.text = s;
//            else if (maybeText is Text ui) ui.text = s;
//        }
//    }
//}
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
        public Image leftAvatar;
        public Image rightAvatar; // 可选，玩家头像

        [Header("Bubbles (电话/上门)")]
        public TypewriterText leftBubble;
        public TypewriterText rightBubble;

        [Header("Chat (短信样式)")]
        public GameObject chatRoot;          // 滚动视图父物体（整个聊天区）
        public Transform chatContent;        // 放消息的 Content
        public GameObject msgLeftPrefab;     // “欠债人消息”预制（里头含 TMP_Text / Text）
        public GameObject msgRightPrefab;    // “玩家消息”预制

        [Header("Options")]
        public Transform optionRoot;
        public Button optionButtonPrefab;

        DialogueStyle _style = DialogueStyle.Bubbles;
        bool _isPhoneOrVisit; // 电话/上门=打字机；短信=瞬显

        string _debtorId;
        ActionCardSO _card;
        ActionStep _currentStep;

        // --- 对外入口 ----------------------------------------------------------

        public void OpenCall(string id, ActionCardSO card)
        {
            _style = DialogueStyle.Bubbles;
            _isPhoneOrVisit = true;
            Open(id, card);
        }

        public void OpenVisit(string id, ActionCardSO card)
        {
            _style = DialogueStyle.Bubbles;
            _isPhoneOrVisit = true;
            Open(id, card);
        }

        public void OpenSMS(string id, ActionCardSO card)
        {
            _style = DialogueStyle.Chat;
            _isPhoneOrVisit = false;
            Open(id, card);
        }

        // 供 ShowLineEffect 兼容调用
        public void ShowLeft(string text)
        {
            if (_style == DialogueStyle.Chat) AddChatLeft(text);
            else
            {
                if (_isPhoneOrVisit) leftBubble?.Play(text);
                else leftBubble?.Show(text);
            }
        }
        public void ShowRight(string text)
        {
            if (_style == DialogueStyle.Chat) AddChatRight(text);
            else
            {
                if (_isPhoneOrVisit) rightBubble?.Play(text);
                else rightBubble?.Show(text);
            }
        }

        public void CloseSelf() => gameObject.SetActive(false);

        // --- 内部流程 ----------------------------------------------------------

        void Open(string debtorId, ActionCardSO card)
        {
            _debtorId = debtorId; _card = card;

            // UI 初始化
            leftBubble?.Clear(); rightBubble?.Clear();
            ClearChat();

            // 切换样式显示区域
            if (chatRoot) chatRoot.SetActive(_style == DialogueStyle.Chat);
            if (leftBubble) leftBubble.gameObject.SetActive(_style == DialogueStyle.Bubbles);
            if (rightBubble) rightBubble.gameObject.SetActive(_style == DialogueStyle.Bubbles);

            // 头像
            if (CaseManager.I.GetSO(debtorId) is DebtorProfileSO so && leftAvatar)
                leftAvatar.sprite = so.portrait;

            // 选择入口 Step
            if (!CaseManager.I.runtimeById.TryGetValue(debtorId, out var rt)) return;
            _currentStep = _card?.GetStepFor(rt);
            ShowStep(_currentStep, rt);

            gameObject.SetActive(true);
        }

        void ShowStep(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();
            if (step == null) { gameObject.SetActive(false); return; }

            // 进入 Step：先执行 onEnterEffects（可用 ShowLineEffect 左/右任意台词）
            step.RunEnter(rt);

            // 再放 NPC 快捷行（左）
            if (!string.IsNullOrEmpty(step.npcLine))
                ShowLeft(step.npcLine);

            // 是否在此步出现选项
            if (step.showOptions) BuildOptions(step, rt);
        }

        void BuildOptions(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();
            if (step.options == null || step.options.Count == 0) return;

            foreach (var opt in step.options)
            {
                if (opt == null) continue;
                if (!opt.ConditionsMet(rt)) continue;

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

                    // 下一步：优先 option 覆盖 → step.nextStep → 默认顺位
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

        // --- Chat 模式工具 ------------------------------------------------------

        void ClearChat()
        {
            if (!chatContent) return;
            for (int i = chatContent.childCount - 1; i >= 0; --i)
                Destroy(chatContent.GetChild(i).gameObject);
        }

        void AddChatLeft(string s)
        {
            if (!msgLeftPrefab || !chatContent)
            {
                // 回退到左泡泡
                leftBubble?.Show(s);
                return;
            }
            var go = Instantiate(msgLeftPrefab, chatContent);
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            var txt = go.GetComponentInChildren<Text>(true);
            (tmp ? (Object)tmp : txt).SetText(s);
            ScrollToBottom();
        }

        void AddChatRight(string s)
        {
            if (!msgRightPrefab || !chatContent)
            {
                rightBubble?.Show(s);
                return;
            }
            var go = Instantiate(msgRightPrefab, chatContent);
            var tmp = go.GetComponentInChildren<TMP_Text>(true);
            var txt = go.GetComponentInChildren<Text>(true);
            (tmp ? (Object)tmp : txt).SetText(s);
            ScrollToBottom();
        }

        void ScrollToBottom()
        {
            // 如果你用的是 ScrollRect，可在这里把 normalizedPosition 设为 (0,0)
            var sr = chatRoot ? chatRoot.GetComponentInChildren<ScrollRect>() : null;
            if (sr) sr.verticalNormalizedPosition = 0f;
        }
    }

    static class TextExt
    {
        public static void SetText(this Object maybeText, string s)
        {
            if (maybeText is TMP_Text tmp) tmp.text = s;
            else if (maybeText is Text ui) ui.text = s;
        }
    }
}
