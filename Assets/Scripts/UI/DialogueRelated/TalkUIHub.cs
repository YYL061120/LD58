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

        // 允许根是“父容器”（没有 Button 组件），真正的 Button 在其子物体里
        [SerializeField] private GameObject optionEntryPrefab;


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

        // —— 调试辅助 —— 
        static int sDbgSeq = 0;           // 全局序号
        int NextSeq() => ++sDbgSeq;
        void D(string msg) => Debug.Log($"[TalkUIHub] {msg}");

        // —— 保护：同一时刻只能跑一个选项协程 —— 
        bool _optionRunning = false;


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
            // —— 防止残留状态干扰 —— 
            _lineQueue.Clear();
            _pendingLines = 0;
            _waitingClick = false;
            _playingQueuedLines = false;

            ClearOptions();
            if (step == null) { CloseSelf(); return; }

            D($"[ShowStep] try stepId={step.stepId}");
            step.RunEnter(rt);
            D($"[ShowStep] enter-done stepId={step.stepId} (npcLine={(string.IsNullOrEmpty(step.npcLine) ? "<empty>" : "OK")})");

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
                var next = ResolveNextStep(_currentStep, null, rt);
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
            if (!optionEntryPrefab)
            {
                Debug.LogError("[TalkUIHub] optionEntryPrefab is NOT set. Please assign a prefab (root may be a parent without Button).");
                return;
            }

            foreach (var opt in step.options)
            {
                if (opt == null || !opt.ConditionsMet(rt)) continue;

                // 1) 实例化“父容器”
                var entryGO = Instantiate(optionEntryPrefab, optionRoot);

                // 2) 找到真正的 Button（根或子节点）
                var btn = FindButtonInChildren(entryGO);
                if (!btn)
                {
                    Debug.LogError("[TalkUIHub] optionEntryPrefab (root or children) must contain a Button.");
                    Destroy(entryGO);
                    continue;
                }

                // 3) 赋值文本：找 TMP_Text / UGUI Text（根或子）
                var tmp = entryGO.GetComponentInChildren<TMP_Text>(true);
                var txt = entryGO.GetComponentInChildren<Text>(true);
                (tmp ? (Object)tmp : txt)?.SetText(opt.optionText);

                // 4) 让父容器可点，并把点击转发给子 Button（避免父子各有一个 Button 造成双触发）
                EnsureRaycastGraphic(entryGO);
                EnsureForwarder(entryGO, btn);

                // 5) 绑定点击逻辑（协程串联：玩家台词→效果台词→进入下一步）
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (_optionRunning) { D("忽略重复点击：已有选项在执行中"); return; }

                    ClearOptions(); // 立刻清掉所有按钮，防止重复点
                    var seq = NextSeq();
                    D($"[OptClick #{seq}] 点击了选项：\"{opt.optionText}\"");
                    StartCoroutine(CoRunOptionThenNext(step, opt, rt, seq));
                });
            }
        }



        IEnumerator CoRunOptionThenNext(ActionStep step, ActionOption opt, CaseRuntime rt, int seq)
        {
            _optionRunning = true;
            SetClickCatcher(true); // 进入阅读模式

            // 1) 玩家台词
            var line = opt.GetPlayerLineForUI();
            D($"[Seq {seq}] ▶ 玩家台词：{line}");
            ShowRight(line);
            yield return null;
            while (rightBubble && rightBubble.IsTyping) yield return null;
            D($"[Seq {seq}] ✔ 玩家台词完毕");

            // 2) 执行效果（把 ShowLineEffect 入队）
            D($"[Seq {seq}] ▶ 执行选项 Effects（可能入队多条台词）");
            opt.ApplyEffects(rt);
            if (opt.triggerCollected) MoneyManager.I?.Collect(rt);

            // 3) 逐句播放队列
            yield return StartCoroutine(CoPlayQueuedLines(seq));

            // 4) 结束或下一步
            if (opt.endsDialogue)
            {
                D($"[Seq {seq}] ✔ 选项结束对话");
                CloseSelf();
                _optionRunning = false;
                yield break;
            }

            _currentStep = ResolveNextStep(step, opt, rt);
            D($"[Seq {seq}] ▶ 进入下一 Step（{(_currentStep != null ? _currentStep.stepId.ToString() : "null")}）");
            ShowStep(_currentStep, rt);

            _optionRunning = false;
        }


        IEnumerator CoPlayQueuedLines(int seq)
        {
            _playingQueuedLines = true;
            try
            {
                int i = 0;
                while (_lineQueue.Count > 0)
                {
                    var ln = _lineQueue.Dequeue();
                    i++;
                    D($"[Seq {seq}] ▶ 队列台词 {i}/{(_lineQueue.Count + 1)}（{(ln.left ? "左" : "右")}）：{ln.text}");

                    if (ln.left) ShowLeft(ln.text); else ShowRight(ln.text);

                    SetClickCatcher(true);
                    _waitingClick = true;

                    // 等这一句“打字完 + 被一次点击消费”
                    while (!AllTypingFinished()) yield return null;

                    D($"[Seq {seq}] ✔ 队列台词 {i} 播放完");
                }
            }
            finally
            {
                _playingQueuedLines = false;
            }
        }

        // 在根或子物体中找 Button
        Button FindButtonInChildren(GameObject root)
        {
            if (!root) return null;
            var b = root.GetComponent<Button>();
            if (!b) b = root.GetComponentInChildren<Button>(true);
            return b;
        }

        // 确保根上有可接收 Raycast 的 Graphic（没有就加一张透明 Image）
        void EnsureRaycastGraphic(GameObject root)
        {
            if (!root) return;
            var g = root.GetComponent<Graphic>();
            if (!g)
            {
                var img = root.AddComponent<Image>();
                var c = img.color; c.a = 0f; img.color = c;
                img.raycastTarget = true;
            }
        }

        // 确保根上有转发器，把点击转发给真正的 Button
        void EnsureForwarder(GameObject root, Button btn)
        {
            if (!root || !btn) return;
            var fwd = root.GetComponent<ForwardClickToButton>();
            if (!fwd) fwd = root.AddComponent<ForwardClickToButton>();
            fwd.target = btn;
        }

        // —— 解析下一步（UI 侧双保险）——
        ActionStep ResolveNextStep(ActionStep cur, ActionOption opt, CaseRuntime rt)
        {
            // 1) 优先：选项自己的 nextStep（若存在）
            if (opt != null)
            {
                // 你可能没有 nextStepId 字段；如果有就用它；没有就仍旧走卡片接口
                var viaCard = _card?.GetNextStep(cur, opt, rt);
                if (viaCard != null) return viaCard;

                // 如果卡片返回空，尽量用 StepId 兜底（需要 ActionCardSO 有按 Id 索引的方法）
                // 假设有 _card.GetStepById(int id)，没有就删掉这段或改成你的命名
                // if (opt.nextStepId > 0) return _card?.GetStepById(opt.nextStepId);
            }

            // 2) 次之：当前 step 的默认 next
            {
                var viaCard = _card?.GetNextStep(cur, null, rt);
                if (viaCard != null) return viaCard;
                // 同理可兜底 step.nextStepId
                // if (cur != null && cur.nextStepId > 0) return _card?.GetStepById(cur.nextStepId);
            }

            // 3) 仍然拿不到：打印详细信息帮助定位
            var curId = cur != null ? cur.stepId.ToString() : "null";
            var optText = opt != null ? opt.optionText : "null";
            D($"[ResolveNextStep] FAIL: curStepId={curId}, optText=\"{optText}\"；" +
              $"卡片返回了 null。检查该选项/Step 的 nextStep 设定和目标 Step 的 Gate 条件。");

            // 可选：把所有 StepId 打印出来，便于比对
            //try
            //{
            //    var allIds = _card != null ? string.Join(",", _card.GetAllStepIds()) : "no-card";
            //    D($"[ResolveNextStep] All step ids in card: {allIds}");
            //}
            //catch { /* 如果没有 GetAllStepIds() 就忽略 */ }

            return null;
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
