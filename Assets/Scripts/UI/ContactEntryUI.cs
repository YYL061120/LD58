// Assets/Scripts/UI/Shared/ContactEntryUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DebtJam
{
    public class ContactEntryUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI")]
        public Image portrait;
        public TMP_Text nameText;
        public CanvasGroup canvasGroup;   // 可选；若为空运行时会自动添加

        // 数据
        public string debtorId { get; private set; }
        public bool interactable { get; private set; } = true;

        System.Action<ContactEntryUI> _onClick;

        void Awake()
        {
            if (!canvasGroup) canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Setup(DebtorProfileSO so, bool allowClick, System.Action<ContactEntryUI> onClick)
        {
            debtorId = so.debtorId;
            interactable = allowClick;
            _onClick = onClick;

            if (portrait) portrait.sprite = so.portrait;
            if (nameText)
            {
                nameText.richText = true;
                nameText.text = so.displayName;
            }

            // 置灰 + 禁点
            canvasGroup.alpha = allowClick ? 1f : 0.45f;
            canvasGroup.interactable = allowClick;
            canvasGroup.blocksRaycasts = true; // 让我们仍然能收到点击去提示原因
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(this);
        }
    }
}
