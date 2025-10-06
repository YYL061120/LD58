// Assets/Scripts/UI/ContactEntryUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DebtJam
{
    /// <summary>
    /// 面板上的一个联系人头像条目：显示头像、名字；可被置灰禁用；可叠加打叉层。
    /// 保留原有 onClick 回调，外部负责发起行动。
    /// </summary>
    public class ContactEntryUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI")]
        public Image portrait;
        public TMP_Text nameText;
        public CanvasGroup canvasGroup;     // 若未手动拖，Awake 时自动补一个
        [Tooltip("打叉/灰显 叠层（可选）")]
        public GameObject crossOutOverlay;

        // 状态
        public string debtorId { get; private set; }
        public bool interactable { get; private set; } = true;

        System.Action<ContactEntryUI> _onClick;

        void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>旧签名（保持兼容）。</summary>
        public void Setup(DebtorProfileSO so, bool allowClick, System.Action<ContactEntryUI> onClick)
        {
            debtorId = so.debtorId;
            _onClick = onClick;

            if (portrait) portrait.sprite = so.portrait;
            if (nameText)
            {
                nameText.richText = true;
                nameText.text = so.displayName;
            }

            SetInteractable(allowClick);
            SetCrossOut(false);
        }

        /// <summary>新签名：可直接传 CaseRuntime，用它判断是否禁用或打叉。</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, bool allowClick, System.Action<ContactEntryUI> onClick)
        {
            Setup(so, allowClick, onClick);

            // 若需要“已结局就打叉”的提示（可选）
            if (rt != null && rt.outcome != CaseOutcome.Pending)
            {
                // 仅提示；是否在列表中显示由上层决定
                SetCrossOut(true);
                SetInteractable(false);
            }
        }

        public void SetInteractable(bool on)
        {
            interactable = on;
            if (!canvasGroup) return;
            canvasGroup.alpha = on ? 1f : 0.45f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = true; // 允许挡住点击，确保灰显也能拦截到
        }

        public void SetCrossOut(bool on)
        {
            if (crossOutOverlay) crossOutOverlay.SetActive(on);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;
            _onClick?.Invoke(this);
        }
    }
}
