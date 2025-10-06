//using System;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;

//namespace DebtJam
//{
//    /// <summary>
//    /// 联系人条目：头像 + 标题(名字) + 副标题(电话/地址) + 可交互/禁用（带灰显和提示）。
//    /// - 兼容 TextMeshPro 和 UGUI Text（自动识别）
//    /// - 提供老接口 Setup(...)，也提供更细粒度的 Set* 接口
//    /// - 可用于 PhonePanel / SMSPanel / VisitPanel
//    /// </summary>
//    public class ContactEntryUI : MonoBehaviour, IPointerClickHandler
//    {
//        [Header("UI Refs")]
//        public Image portrait;
//        [Tooltip("优先使用 TMP_Text；若为空会回退到 UGUI Text。")]
//        public TMP_Text titleTMP;
//        public Text titleText;
//        [Tooltip("优先使用 TMP_Text；若为空会回退到 UGUI Text。")]
//        public TMP_Text subtitleTMP;
//        public Text subtitleText;

//        [Header("State Visuals (可选)")]
//        [Tooltip("禁用时覆盖的一层半透明遮罩（可选，不填则不显示遮罩）。")]
//        public GameObject disabledMask;
//        [Tooltip("禁用时显示的小图标（例如锁/禁止）（可选）。")]
//        public GameObject lockIcon;
//        [Tooltip("可选：置灰材质（Sprite/UGUI 用，禁用时套这个材质）。不填则仅用颜色淡化。")]
//        public Material grayscaleMaterial;

//        [Header("Click (可选)")]
//        [Tooltip("如果条目本身或子物体上有 Button，则会优先用 Button 的 onClick；否则用父物体点击。")]
//        public Button clickableButton; // 允许挂在子物体或根物体，Awake 自动搜

//        // 运行期
//        Action _onClick;
//        bool _interactable = true;
//        string _disabledReason;

//        void Awake()
//        {
//            // 自动抓取 Button（根或子节点）
//            if (!clickableButton) clickableButton = GetComponent<Button>();
//            if (!clickableButton) clickableButton = GetComponentInChildren<Button>(true);

//            if (clickableButton)
//            {
//                clickableButton.onClick.RemoveAllListeners();
//                clickableButton.onClick.AddListener(HandleClick);
//            }

//            // 如果整个条目需要可点击（即使没有 Button），确保有 RaycastTarget
//            var g = GetComponent<Graphic>();
//            if (!g)
//            {
//                // 为了能接收点击事件（IPointerClickHandler），自动加一个透明 Image 当射线接收器
//                var img = gameObject.AddComponent<Image>();
//                var c = img.color; c.a = 0f; img.color = c;
//                img.raycastTarget = true;
//            }

//            RefreshVisuals();
//        }

//        #region ======= 兼容你之前的老接口 =======

//        /// <summary>
//        /// 你之前已有的：一把梭设置。保留并增强。
//        /// subtitleText：在 Phone/SMS 面板传“格式化后的电话”；在 Visit 面板传“格式化后的地址”。
//        /// canInteract：是否可点；onClicked：点击回调。
//        /// </summary>
//        public void Setup(CaseRuntime rt, DebtorProfileSO so, string subtitleText, bool canInteract, Action onClicked)
//        {
//            if (so)
//            {
//                SetPortrait(so.portrait);
//                SetTitle(so.displayName);
//            }
//            else
//            {
//                SetPortrait(null);
//                SetTitle(rt != null ? rt.displayName : "—");
//            }

//            SetSubtitle(string.IsNullOrWhiteSpace(subtitleText) ? "—" : subtitleText);
//            SetInteractable(canInteract);
//            Bind(onClicked);
//        }

//        /// <summary>
//        /// 一键为某个“行动面板”配置条目；
//        /// 会根据 ActionType 自动选择显示 Phone / Address，并判定可点击与否。
//        /// </summary>
//        public void SetupForAction(ActionType action, CaseRuntime rt, DebtorProfileSO so, Action onClicked)
//        {
//            if (rt == null) { Setup(null, so, "—", false, null); return; }

//            SetPortrait(so ? so.portrait : null);
//            SetTitle(rt.displayName);

//            switch (action)
//            {
//                case ActionType.Call:
//                case ActionType.SMS:
//                {
//                    var phone = rt.phoneNumber ?? "";
//                    // 若你引入了 ContactFormat 工具，优先美化显示
//                    try { phone = ContactFormat.PrettyPhone(phone); } catch { }
//                    SetSubtitle(string.IsNullOrWhiteSpace(phone) ? "—" : phone);
//                    SetInteractable(rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber));
//                    break;
//                }
//                case ActionType.Visit:
//                {
//                    var addr = rt.address ?? "";
//                    try { addr = ContactFormat.PrettyAddress(addr); } catch { }
//                    SetSubtitle(string.IsNullOrWhiteSpace(addr) ? "—" : addr);
//                    SetInteractable(rt.hasAddress && !string.IsNullOrWhiteSpace(rt.address));
//                    break;
//                }
//                default:
//                    SetSubtitle("—");
//                    SetInteractable(false);
//                    break;
//            }

//            Bind(onClicked);
//        }

//        #endregion

//        #region ======= 细粒度 API（新的/保留） =======

//        public void Bind(Action onClicked)
//        {
//            _onClick = onClicked;
//        }

//        public void SetTitle(string s)
//        {
//            if (titleTMP) titleTMP.text = s ?? "";
//            if (titleText) titleText.text = s ?? "";
//        }

//        public void SetSubtitle(string s)
//        {
//            if (subtitleTMP) subtitleTMP.text = s ?? "";
//            if (subtitleText) subtitleText.text = s ?? "";
//        }

//        public void SetPortrait(Sprite sp)
//        {
//            if (!portrait) return;
//            portrait.sprite = sp;
//            portrait.enabled = (sp != null);
//        }

//        /// <summary>
//        /// 设置能否交互（会自动处理按钮可点，遮罩、置灰等）
//        /// </summary>
//        public void SetInteractable(bool on, string reason = null)
//        {
//            _interactable = on;
//            _disabledReason = reason;

//            if (clickableButton) clickableButton.interactable = on;

//            // 头像置灰 + 文字半透明 + 遮罩/锁图标
//            if (portrait)
//            {
//                if (on)
//                {
//                    portrait.material = null;
//                    var c = portrait.color; c.a = 1f; portrait.color = c;
//                }
//                else
//                {
//                    if (grayscaleMaterial) portrait.material = grayscaleMaterial;
//                    var c = portrait.color; c.a = 0.6f; portrait.color = c;
//                }
//            }

//            SetTextAlpha(titleTMP, titleText, on ? 1f : 0.6f);
//            SetTextAlpha(subtitleTMP, subtitleText, on ? 1f : 0.5f);

//            if (disabledMask) disabledMask.SetActive(!on);
//            if (lockIcon) lockIcon.SetActive(!on);
//        }

//        /// <summary>
//        /// 可选：单独设置禁用原因（仅存储；具体显示你可以在外部读取并弹 Toast/Tip）
//        /// </summary>
//        public void SetDisabledReason(string reason)
//        {
//            _disabledReason = reason;
//        }

//        #endregion

//        #region ======= 事件与工具 =======

//        public void OnPointerClick(PointerEventData eventData)
//        {
//            // 若有 Button 且可交互，直接交给 Button 处理（避免重复触发）
//            if (clickableButton)
//            {
//                if (clickableButton.interactable)
//                    clickableButton.onClick?.Invoke();
//                return;
//            }

//            // 无 Button：使用父节点点击
//            if (_interactable)
//            {
//                _onClick?.Invoke();
//            }
//            else
//            {
//                // 这里可以按需弹提示：Debug.Log($"不可点击：{_disabledReason}");
//            }
//        }

//        void HandleClick()
//        {
//            if (!_interactable) return;
//            _onClick?.Invoke();
//        }

//        void RefreshVisuals()
//        {
//            // 初始化时根据 _interactable 刷一次（如果 Awake 时没有被 SetInteractable 明确设置）
//            SetInteractable(_interactable, _disabledReason);
//        }

//        static void SetTextAlpha(TMP_Text tmp, Text ui, float a)
//        {
//            if (tmp)
//            {
//                var c = tmp.color; c.a = a; tmp.color = c;
//            }
//            if (ui)
//            {
//                var c = ui.color; c.a = a; ui.color = c;
//            }
//        }

//        #endregion
//    }
//}

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DebtJam
{
    /// <summary>
    /// 联系人条目：头像 + 标题(名字) + 副标题(电话/地址) + 可交互/禁用（带灰显和提示）
    /// - 兼容 TextMeshPro 和 UGUI Text
    /// - 向后兼容：多个 Setup 重载、BindClick、debtorId 字段等
    /// - 可用于 Phone/SMS/Visit
    /// </summary>
    public class ContactEntryUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Refs")]
        public Image portrait;
        public TMP_Text titleTMP;
        public Text titleText;
        public TMP_Text subtitleTMP;
        public Text subtitleText;

        [Header("State Visuals (可选)")]
        public GameObject disabledMask;
        public GameObject lockIcon;
        public Material grayscaleMaterial;

        [Header("Click (可选)")]
        public Button clickableButton;

        // ====== 旧项目可能会访问的字段 ======
        [NonSerialized] public string debtorId;      // 旧代码可能直接读
        [NonSerialized] public CaseRuntime runtime;  // 旧代码可能直接读

        // 运行期
        Action _onClick;
        bool _interactable = true;
        string _disabledReason;

        void Awake()
        {
            // 自动抓取 Button（根或子）
            if (!clickableButton) clickableButton = GetComponent<Button>();
            if (!clickableButton) clickableButton = GetComponentInChildren<Button>(true);

            if (clickableButton)
            {
                clickableButton.onClick.RemoveAllListeners();
                clickableButton.onClick.AddListener(HandleClick);
            }

            // 保证可被点到（IPointerClickHandler）
            var g = GetComponent<Graphic>();
            if (!g)
            {
                var img = gameObject.AddComponent<Image>();
                var c = img.color; c.a = 0f; img.color = c;
                img.raycastTarget = true;
            }

            RefreshVisuals();
        }

        // ========= 向后兼容：旧签名 =========
        /// <summary>旧：最简单的 Setup（默认可交互、无点击回调）。</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, string subtitleText)
            => Setup(rt, so, subtitleText, true, null);

        /// <summary>旧：默认无点击回调。</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, string subtitleText, bool canInteract)
            => Setup(rt, so, subtitleText, canInteract, null);

        /// <summary>旧：有 rt/so 但无 subtitle 的调用。</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so)
        {
            var sub = rt != null ? (rt.phoneNumber ?? rt.address ?? "—") : "—";
            Setup(rt, so, sub, true, null);
        }

        /// <summary>旧：有 rt/so 与 onClick，但不传副标题（自动挑一个）。</summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, Action onClicked)
        {
            var sub = rt != null ? (rt.phoneNumber ?? rt.address ?? "—") : "—";
            Setup(rt, so, sub, true, onClicked);
        }

        /// <summary>旧：绑定点击（别名）。</summary>
        public void BindClick(Action onClicked) => Bind(onClicked);

        // ========= 新签名（完整） =========
        /// <summary>
        /// 完整 Setup：你可传入想显示的副标题（电话/地址均可）、是否可交互、点击回调
        /// </summary>
        public void Setup(CaseRuntime rt, DebtorProfileSO so, string subtitle, bool canInteract, Action onClicked)
        {
            runtime = rt;
            debtorId = rt != null ? rt.debtorId : null;

            if (so)
            {
                SetPortrait(so.portrait);
                SetTitle(so.displayName);
            }
            else
            {
                SetPortrait(null);
                SetTitle(rt != null ? rt.displayName : "—");
            }

            SetSubtitle(string.IsNullOrWhiteSpace(subtitle) ? "—" : subtitle);
            SetInteractable(canInteract);
            Bind(onClicked);
        }

        /// <summary>
        /// 一键配置到具体行动（Call/SMS/Visit），会自动选择显示 Phone/Address 和可用性
        /// </summary>
        public void SetupForAction(ActionType action, CaseRuntime rt, DebtorProfileSO so, Action onClicked)
        {
            runtime = rt;
            debtorId = rt != null ? rt.debtorId : null;

            SetPortrait(so ? so.portrait : null);
            SetTitle(rt != null ? rt.displayName : "—");

            bool can = false;
            string sub = "—";

            if (rt != null)
            {
                switch (action)
                {
                    case ActionType.Call:
                    case ActionType.SMS:
                        sub = rt.phoneNumber ?? "";
                        try { sub = ContactFormat.PrettyPhone(sub); } catch { }
                        can = rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber);
                        break;

                    case ActionType.Visit:
                        sub = rt.address ?? "";
                        try { sub = ContactFormat.PrettyAddress(sub); } catch { }
                        can = rt.hasAddress && !string.IsNullOrWhiteSpace(rt.address);
                        break;
                }
            }

            SetSubtitle(string.IsNullOrWhiteSpace(sub) ? "—" : sub);
            SetInteractable(can);
            Bind(onClicked);
        }

        // ========= 细粒度 API =========
        public void Bind(Action onClicked) => _onClick = onClicked;

        public void SetTitle(string s)
        {
            if (titleTMP) titleTMP.text = s ?? "";
            if (titleText) titleText.text = s ?? "";
        }

        public void SetSubtitle(string s)
        {
            if (subtitleTMP) subtitleTMP.text = s ?? "";
            if (subtitleText) subtitleText.text = s ?? "";
        }

        public void SetPortrait(Sprite sp)
        {
            if (!portrait) return;
            portrait.sprite = sp;
            portrait.enabled = (sp != null);
        }

        public void SetInteractable(bool on, string reason = null)
        {
            _interactable = on;
            _disabledReason = reason;
            if (clickableButton) clickableButton.interactable = on;

            if (portrait)
            {
                if (on)
                {
                    portrait.material = null;
                    var c = portrait.color; c.a = 1f; portrait.color = c;
                }
                else
                {
                    if (grayscaleMaterial) portrait.material = grayscaleMaterial;
                    var c = portrait.color; c.a = 0.6f; portrait.color = c;
                }
            }

            SetTextAlpha(titleTMP, titleText, on ? 1f : 0.6f);
            SetTextAlpha(subtitleTMP, subtitleText, on ? 1f : 0.5f);

            if (disabledMask) disabledMask.SetActive(!on);
            if (lockIcon) lockIcon.SetActive(!on);
        }

        public void SetDisabledReason(string reason) => _disabledReason = reason;

        // ========= 事件 & 工具 =========
        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickableButton)
            {
                if (clickableButton.interactable)
                    clickableButton.onClick?.Invoke();
                return;
            }

            if (_interactable)
            {
                _onClick?.Invoke();
            }
            else
            {
                // 可在外部用 _disabledReason 做提示
                // Debug.Log($"[ContactEntryUI] 点击了禁用条目：{_disabledReason}");
            }
        }

        void HandleClick()
        {
            if (!_interactable) return;
            _onClick?.Invoke();
        }

        void RefreshVisuals() => SetInteractable(_interactable, _disabledReason);

        static void SetTextAlpha(TMP_Text tmp, Text ui, float a)
        {
            if (tmp)
            {
                var c = tmp.color; c.a = a; tmp.color = c;
            }
            if (ui)
            {
                var c = ui.color; c.a = a; ui.color = c;
            }
        }
    }
}

