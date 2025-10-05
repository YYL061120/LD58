// Assets/Scripts/UI/Dialogue/DialogueBubblesUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DebtJam
{
    public class DialogueBubblesUI : MonoBehaviour
    {
        // 供 Effect 直接调用
        public static DialogueBubblesUI Current { get; private set; }

        [Header("Root")]
        public GameObject root;

        [Header("Left (Debtor)")]
        public Image leftPortrait;
        public TMP_Text leftName;
        public TypewriterText leftTyper;   // 直接挂在左气泡文本上

        [Header("Right (Player)")]
        public Image rightPortrait;
        public TMP_Text rightName;
        public TypewriterText rightTyper;  // 直接挂在右气泡文本上

        [Header("Player Options")]
        public Transform optionsRoot;
        public DialogueOptionButton optionPrefab; // 你现有的按钮预制（有一个TMP_Text）

        [Header("Style")]
        public string playerDisplayName = "你";

        CaseRuntime current;
        DebtorProfileSO so;
        bool _typewriterOn = true; // 电话/上门=true；短信=false

        ActionCardSO _card;
        ActionStep _step;

        void Awake()
        {
            if (!root) root = gameObject;
            Hide();
            Current = this;
        }

        public void Hide() => root.SetActive(false);
        public void Show() => root.SetActive(true);

        // === 对外入口 ===
        public void BeginCall(CaseRuntime rt) { Begin(rt, ActionType.Call, true); }
        public void BeginVisit(CaseRuntime rt) { Begin(rt, ActionType.Visit, true); }
        public void BeginSMS(CaseRuntime rt) { Begin(rt, ActionType.SMS, false); }

        void Begin(CaseRuntime rt, ActionType type, bool typewriter)
        {
            current = rt;
            _typewriterOn = typewriter;

            so = CaseManager.I.GetSO(rt.debtorId);
            if (!so) return;

            // 头像/名字
            if (leftPortrait) leftPortrait.sprite = so.portrait;
            if (leftName) leftName.text = so.displayName;
            if (rightName) rightName.text = playerDisplayName;

            // 清空文字
            leftTyper?.Clear();
            rightTyper?.Clear();

            // 打开 UI
            Show();

            // 拿该行动对应的卡
            _card = type switch
            {
                ActionType.Call => so.callCard,
                ActionType.SMS => so.smsCard,
                ActionType.Visit => so.visitCard,
                _ => null
            };

            if (!_card)
            {
                ShowLeft("……");
                Finish(false);
                return;
            }

            // 入口 Step
            _step = _card.GetStepFor(current);
            ShowStep(_step);
        }

        // === 核心：显示一步（执行 onEnterEffects → 可选 npcLine → (可选) 选项） ===
        void ShowStep(ActionStep step)
        {
            if (step == null) { Finish(false); return; }

            // 进入该步：先执行效果（里面可以用 ShowLineEffect 写任意多句，左/右都行）
            step.RunEnter(current);

            // 快捷 NPC 行（左）
            if (!string.IsNullOrEmpty(step.npcLine))
                ShowLeft(step.npcLine);

            // 选项
            ClearOptions();
            if (step.showOptions && step.options != null && step.options.Count > 0)
            {
                foreach (var opt in step.options)
                {
                    if (opt == null) continue;
                    if (!opt.ConditionsMet(current)) continue;

                    var btn = Instantiate(optionPrefab, optionsRoot);
                    btn.Setup(opt.optionText, () =>
                    {
                        // 玩家台词（右）
                        var playerLine = opt.GetPlayerLineForUI();
                        ShowRight(playerLine);

                        // 执行效果（Reveal/Milestone/锁定/收款…）
                        opt.ApplyEffects(current);
                        if (opt.triggerCollected) MoneyManager.I?.Collect(current);

                        if (opt.endsDialogue)
                        {
                            Finish(false);
                            return;
                        }

                        // 下一步：优先 option 指定；否则 step.nextStep；否则顺序下一
                        _step = _card.GetNextStep(step, opt, current);
                        ShowStep(_step);
                    });
                }
            }
        }

        void ClearOptions()
        {
            if (!optionsRoot) return;
            for (int i = optionsRoot.childCount - 1; i >= 0; --i)
                Destroy(optionsRoot.GetChild(i).gameObject);
        }

        // === 给 Effects 调的友好方法（自动判断：短信瞬显 / 电话打字） ===
        public void ShowLeft(string line)
        {
            if (!leftTyper) return;
            if (_typewriterOn) leftTyper.Play(line);
            else leftTyper.Show(line);
        }

        public void ShowRight(string line)
        {
            if (!rightTyper) return;
            if (_typewriterOn) rightTyper.Play(line);
            else rightTyper.Show(line);
        }

        void Finish(bool collected)
        {
            // 收款已在 opt.triggerCollected 时处理；这里通常只是关 UI
            Hide();
            Current = null;
        }
    }
}
