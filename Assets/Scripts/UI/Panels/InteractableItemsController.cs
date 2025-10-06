// Assets/Scripts/UI/Interactables/InteractableItemsController.cs
using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    public class InteractableItemsController : MonoBehaviour
    {
        public static InteractableItemsController I { get; private set; }

        [Header("Panels")]
        public GameObject phonePanel;     // 电话面板
        public GameObject visitPanel;     // 上门面板
        public GameObject screenPanel;    // 屏幕面板
        public GameObject smsPanel;       // 从屏幕里打开
        public GameObject profilePanel;   // 从屏幕里打开

        [Header("Optional")]
        public GameObject[] othersToHideWhenScreenOpen;

        [Header("Interactable Objects (scene items)")]
        [Tooltip("把橙色电话、红色钥匙、屏幕 这三个物体拖进来（物体上需要有 BoxCollider2D 或 BoxCollider）。")]
        public GameObject[] interactableObjects;

        // ===== 缓存（支持 2D / 3D）=====
        readonly List<Collider2D> _colliders2D = new();
        readonly List<Collider> _colliders3D = new();
        readonly List<Behaviour> _hoverScripts = new(); // e.g. HoverScaleAndClick

        // 面板开关集合（任意面板打开→禁用交互）
        readonly HashSet<GameObject> _openedPanels = new();

        // 额外锁（对话/过场/动画等时机使用）
        int _extraLockCount = 0;

        public bool IsLocked => _extraLockCount > 0 || _openedPanels.Count > 0;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;

            // 缓存所有交互对象的组件（包含子物体）
            _colliders2D.Clear(); _colliders3D.Clear(); _hoverScripts.Clear();

            if (interactableObjects != null)
            {
                foreach (var go in interactableObjects)
                {
                    if (!go) continue;
                    _colliders2D.AddRange(go.GetComponentsInChildren<Collider2D>(true));
                    _colliders3D.AddRange(go.GetComponentsInChildren<Collider>(true));

                    // 尝试抓取你的悬停放大脚本（或任何你想禁用的“Hover类脚本”）
                    var hovers = go.GetComponentsInChildren<HoverScaleAndClick>(true);
                    foreach (var h in hovers) if (h) _hoverScripts.Add(h);
                }
            }

            // 自动挂监听
            AutoAttachWatcher(phonePanel);
            AutoAttachWatcher(visitPanel);
            AutoAttachWatcher(screenPanel);
            AutoAttachWatcher(smsPanel);
            AutoAttachWatcher(profilePanel);
        }

        private void Update()
        {
            Debug.Log(InteractableItemsController.I?.IsLocked);
        }

        void AutoAttachWatcher(GameObject panel)
        {
            if (!panel) return;
            var watcher = panel.GetComponent<PanelOpenCloseNotifier>();
            if (!watcher) watcher = panel.AddComponent<PanelOpenCloseNotifier>();
            watcher.controller = this; // 关联回调
        }

        // ========== 对外公开的“开关”方法（供 HoverScaleAndClick / ScreenPanelButtons 调用） ==========

        public void OpenPhone() { SetActiveSafe(phonePanel, true); }
        public void OpenVisit() { SetActiveSafe(visitPanel, true); }
        public void OpenScreen()
        {
            SetActiveSafe(screenPanel, true);
            if (othersToHideWhenScreenOpen != null)
                foreach (var go in othersToHideWhenScreenOpen) SetActiveSafe(go, false);
        }
        public void CloseScreen() { SetActiveSafe(screenPanel, false); }

        public void OpenSMSFromScreen() { SetActiveSafe(smsPanel, true); }
        public void OpenProfileFromScreen() { SetActiveSafe(profilePanel, true); }

        void SetActiveSafe(GameObject go, bool active)
        {
            if (!go) return;
            if (go.activeSelf == active) return;
            go.SetActive(active);
            // 实际的开关处理由 PanelOpenCloseNotifier 的 OnEnable/OnDisable 通知来驱动
        }

        // ========== 给面板观察者调用（任何方式开/关面板都会回调到这里） ==========

        internal void NotifyPanelOpened(GameObject panel)
        {
            if (panel == null) return;
            if (_openedPanels.Add(panel))
                Recalc(); // 统一由 Recalc 决定是否禁用
        }

        internal void NotifyPanelClosed(GameObject panel)
        {
            if (panel == null) return;
            _openedPanels.Remove(panel);
            Recalc();
        }

        // ========== 供对话/动画等调用的额外锁（引用计数）==========

        public void Lock(string reason = null)
        {
            _extraLockCount++;
            Recalc();
            // Debug.Log($"[Interactables] LOCK({_extraLockCount}) {reason}");
        }

        public void Unlock(string reason = null)
        {
            _extraLockCount = Mathf.Max(0, _extraLockCount - 1);
            Recalc();
            // Debug.Log($"[Interactables] UNLOCK({_extraLockCount}) {reason}");
        }

        // ========== 统一计算是否可交互 ==========
        void Recalc()
        {
            bool allow = (_openedPanels.Count == 0) && (_extraLockCount == 0);
            SetInteractorsEnabled(allow);
        }

        // ========== 真正控制 collider/hover 开关 ==========
        void SetInteractorsEnabled(bool enabled)
        {
            foreach (var c in _colliders2D) if (c) c.enabled = enabled;
            foreach (var c in _colliders3D) if (c) c.enabled = enabled;
            foreach (var h in _hoverScripts) if (h) h.enabled = enabled;
        }

        // 你原有的“全局开关”方法保留（不走缓存，按需扫描当前节点）
        public void SetAllWorldInteractables(bool on)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>(true)) col.enabled = on;
            foreach (var col in GetComponentsInChildren<Collider>(true)) col.enabled = on;
            foreach (var h in GetComponentsInChildren<HoverScaleAndClick>(true)) h.enabled = on;
        }
    }
}
