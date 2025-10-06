// Assets/Scripts/UI/TalkUIHub.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

        // 运行期状态
        DialogueStyle _style = DialogueStyle.Bubbles;
        bool _isPhoneOrVisit;      // 电话/上门=逐字；短信=瞬显
        string _debtorId;
        ActionCardSO _card;
        ActionStep _currentStep;
        bool _playingQueuedLines;   // 当前是否在逐句播放队列（点击只能用来消费队列，不允许推进Step）


        public InteractableItemsController items;   // 在 Inspector 里拖场景里的控制器
        public GameObject clickCatcher;             // 全屏透明 Button（onClick 调用 Advance）
        public event System.Action OnClosed;

        bool _waitingClick;          // 本句播完后，是否等待点击继续
        int _pendingLines;           // 由 ShowLeft/Right 增加，用于“这句是否已完全显示”

        // —— 新增：把 ShowLineEffect 的台词排队，逐句播 —— 
        struct PendingLine { public bool left; public string text; }
        readonly Queue<PendingLine> _lineQueue = new();

        public void QueueLine(bool onLeft, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _lineQueue.Enqueue(new PendingLine { left = onLeft, text = text });
        }

        // ===== 对外入口 =====
        public void OpenCall(string id, ActionCardSO card) { _style = DialogueStyle.Bubbles; _isPhoneOrVisit = true; Open(id, card); }
        public void OpenVisit(string id, ActionCardSO card) { _style = DialogueStyle.Bubbles; _isPhoneOrVisit = true; Open(id, card); }
        public void OpenSMS(string id, ActionCardSO card) { _style = DialogueStyle.Chat; _isPhoneOrVisit = false; Open(id, card); }

        // ===== 兼容 ShowLineEffect：立即显示一条并设置“等待点击” =====
        public void ShowLeft(string text)
        {
            if (_style == DialogueStyle.Chat) { AddChatLeft(text); return; }

            leftBubble?.Clear();
            if (_isPhoneOrVisit) leftBubble?.Play(text);
            else leftBubble?.Show(text);

            _waitingClick = true;
            _pendingLines++;
        }

        public void ShowRight(string text)
        {
            if (_style == DialogueStyle.Chat) { AddChatRight(text); return; }

            rightBubble?.Clear();
            if (_isPhoneOrVisit) rightBubble?.Play(text);
            else rightBubble?.Show(text);

            _waitingClick = true;
            _pendingLines++;
        }

        public void CloseSelf()
        {
            SetClickCatcher(false);
            items?.SetAllWorldInteractables(true);
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        // ===== 内部流程 =====
        void Open(string debtorId, ActionCardSO card)
        {
            if (leftBubble) leftBubble.gameObject.SetActive(true);
            if (rightBubble) rightBubble.gameObject.SetActive(true);

            items?.SetAllWorldInteractables(false);
            SetClickCatcher(true);
            gameObject.SetActive(true);

            _debtorId = debtorId; _card = card;

            leftBubble?.Clear(); rightBubble?.Clear();
            ClearChat();
            _lineQueue.Clear();
            _pendingLines = 0;
            _waitingClick = false;

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
        }

        void ShowStep(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();
            if (step == null) { CloseSelf(); return; }

            step.RunEnter(rt);

            if (!string.IsNullOrEmpty(step.npcLine))
                ShowLeft(step.npcLine);

            // 若该 Step 要出选项 → 立刻关掉点击捕手，让玩家点按钮
            if (step.showOptions && step.options != null && step.options.Count > 0)
            {
                BuildOptions(step, rt);
                SetClickCatcher(false);
                _waitingClick = false; // 由按钮驱动
            }
            else
            {
                // 没选项 → 点击推进
                _waitingClick = true;
                SetClickCatcher(true);
            }
        }

        // 点击推进
        public void Advance()
        {
            // 1) 若任何一侧正在打字，先跳到末尾（这次点击只用来跳字）
            if (leftBubble && leftBubble.IsTyping) { leftBubble.SkipToEnd(); return; }
            if (rightBubble && rightBubble.IsTyping) { rightBubble.SkipToEnd(); return; }

            // 2) 如果正处于“逐句播放 ShowLineEffect 队列”的阶段，
            //    这次点击只用于“结束当前句/进入下一句”，绝不推进 Step。
            if (_playingQueuedLines)
            {
                // 每次显示一条我们都会 _pendingLines++；点击就把它减掉
                if (_pendingLines > 0) _pendingLines--;
                // 直接返回：由 CoPlayQueuedLines() 的 while 循环继续驱动
                return;
            }

            // 3) 非队列播放阶段：按原有规则处理
            if (_pendingLines > 0)
            {
                _pendingLines--;
                if (_pendingLines > 0) return; // 还有外部台词未播完，继续等
            }

            if (!_waitingClick) return;

            _waitingClick = false;

            if (_currentStep != null)
            {
                var rt = CaseManager.I.runtimeById[_debtorId];

                if (_currentStep.showOptions && _currentStep.options != null && _currentStep.options.Count > 0)
                {
                    BuildOptions(_currentStep, rt);
                    SetClickCatcher(false);
                    return;
                }

                // 没选项 → 进入下一步
                var next = _card.GetNextStep(_currentStep, null, rt);
                if (next != null)
                {
                    _currentStep = next;
                    ShowStep(_currentStep, rt);
                }
                else
                {
                    CloseSelf();
                }
            }
        }


        // TalkUIHub.cs 片段 —— 完整替换 BuildOptions 方法
        void BuildOptions(ActionStep step, CaseRuntime rt)
        {
            ClearOptions();

            if (step == null) return;

            if (step.options == null || step.options.Count == 0)
            {
                Debug.Log("[TalkUIHub] No options on this step.");
                return;
            }

            if (!optionRoot)
            {
                Debug.LogError("[TalkUIHub] optionRoot is NOT set. Please assign a container Transform.");
                return;
            }
            if (!optionButtonPrefab)
            {
                Debug.LogError("[TalkUIHub] optionButtonPrefab is NOT set. Please assign a Button prefab.");
                return;
            }

            foreach (var opt in step.options)
            {
                if (opt == null || !opt.ConditionsMet(rt)) continue;

                // 现在允许 Button 在根物体或子物体：都能工作
                var entry = Instantiate(optionButtonPrefab.gameObject, optionRoot);
                // 1) 找到真正的 Button（根或子节点）
                var btn = entry.GetComponent<Button>() ?? entry.GetComponentInChildren<Button>(true);

                if (!btn)
                {
                    Debug.LogError("[TalkUIHub] optionButtonPrefab (root or children) must contain a Button.");
                    Destroy(entry);
                    continue;
                }

                // 2) 给文本赋值：搜索 TMP_Text/UGUI Text（根或子节点）
                var tmp = entry.GetComponentInChildren<TMP_Text>(true);
                var txt = entry.GetComponentInChildren<Text>(true);
                (tmp ? (Object)tmp : txt)?.SetText(opt.optionText);

                // 3) 让“父物体也能点到”
                //    - 在根上挂 ForwardClickToButton，把 target 指向按钮
                //    - 根上若没有 Graphic（接收射线），自动加一张透明 Image 以接收点击
                var forward = entry.GetComponent<ForwardClickToButton>();
                if (!forward) forward = entry.AddComponent<ForwardClickToButton>();
                forward.target = btn;

                var g = entry.GetComponent<Graphic>();
                if (!g)
                {
                    var img = entry.AddComponent<Image>();
                    var c = img.color; c.a = 0f; img.color = c; // 透明但可点击
                    img.raycastTarget = true;
                }

                // 4) 绑定点击逻辑（协程串联：玩家台词→效果台词→进入下一步）
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    // 点击后立刻清掉所有按钮，防止重复点
                    ClearOptions();
                    StartCoroutine(CoRunOptionThenNext(step, opt, rt));
                });
            }
        }


        IEnumerator CoRunOptionThenNext(ActionStep step, ActionOption opt, CaseRuntime rt)
        {
            // 立刻把选项清掉，避免还能点到其它按钮
            ClearOptions();
            SetClickCatcher(true); // 进入“阅读/点击推进”模式

            // 1) 先播放玩家台词（右）
            var line = opt.GetPlayerLineForUI();
            ShowRight(line);
            yield return null;
            while (rightBubble && rightBubble.IsTyping) yield return null;

            // 2) 执行效果（ShowLineEffect 只负责入队）
            opt.ApplyEffects(rt);
            if (opt.triggerCollected) MoneyManager.I?.Collect(rt);

            // 3) 逐句播放队列（每句都要点击推进）
            yield return StartCoroutine(CoPlayQueuedLines());

            // 4) 结束还是进下一步
            if (opt.endsDialogue)
            {
                CloseSelf();
                yield break;
            }

            _currentStep = _card.GetNextStep(step, opt, rt);
            ShowStep(_currentStep, rt);
        }

        IEnumerator CoPlayQueuedLines()
        {
            _playingQueuedLines = true;   // ▲ 加锁：期间点击只消费队列
            try
            {
                while (_lineQueue.Count > 0)
                {
                    var ln = _lineQueue.Dequeue();
                    if (ln.left) ShowLeft(ln.text); else ShowRight(ln.text);

                    SetClickCatcher(true);
                    _waitingClick = true;

                    // 等到“这句”被点击推进且完全结束（包括打字结束 & pending 归零）
                    while (!AllTypingFinished()) yield return null;
                }
            }
            finally
            {
                _playingQueuedLines = false; // ▲ 解锁：后续点击才能推进 Step
            }
        }


        void ClearOptions()
        {
            if (!optionRoot) return;
            for (int i = optionRoot.childCount - 1; i >= 0; --i)
                Destroy(optionRoot.GetChild(i).gameObject);
        }

        // —— Chat 模式工具 —— 
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

        void SetClickCatcher(bool on)
        {
            if (!clickCatcher) return;
            if (clickCatcher.activeSelf == on) return;
            clickCatcher.SetActive(on);
        }

        bool AllTypingFinished()
        {
            bool leftDone = (leftBubble == null || !leftBubble.IsTyping);
            bool rightDone = (rightBubble == null || !rightBubble.IsTyping);
            return leftDone && rightDone && _pendingLines <= 0;
        }
    }

    // TMP 或 UGUI Text 二选一赋值
    static class TextExt
    {
        public static void SetText(this Object maybeText, string s)
        {
            if (maybeText is TMP_Text tmp) tmp.text = s;
            else if (maybeText is Text ui) ui.text = s;
        }
    }
}
