using UnityEngine;
using System.Collections.Generic;

namespace DebtJam
{
    public class InteractableItemsController : MonoBehaviour
    {
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

        // 缓存到的 collider 列表（支持 2D / 3D）
        readonly List<Collider2D> _colliders2D = new();
        readonly List<Collider> _colliders3D = new();

        // 正在打开的“会屏蔽交互”的面板集合
        readonly HashSet<GameObject> _openedPanels = new();

        void Awake()
        {
            // 缓存 Collider
            _colliders2D.Clear(); _colliders3D.Clear();
            if (interactableObjects != null)
            {
                foreach (var go in interactableObjects)
                {
                    if (!go) continue;
                    _colliders2D.AddRange(go.GetComponents<Collider2D>());
                    _colliders3D.AddRange(go.GetComponents<Collider>());
                }
            }

            // 给目标面板自动加“开关观察者”（若没有）
            AutoAttachWatcher(phonePanel);
            AutoAttachWatcher(visitPanel);
            AutoAttachWatcher(screenPanel);
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
            // 不需要手动调用 Notify —— 由 PanelOpenCloseNotifier 在 OnEnable/OnDisable 自动回调
        }

        // ========== 提供给面板观察者调用的回调（任何途径开关面板，都能正确联动） ==========

        internal void NotifyPanelOpened(GameObject panel)
        {
            if (panel == null) return;
            if (_openedPanels.Add(panel))
            {
                // 有至少一个面板打开 → 关闭交互器
                SetInteractorsEnabled(false);
            }
        }

        internal void NotifyPanelClosed(GameObject panel)
        {
            if (panel == null) return;
            _openedPanels.Remove(panel);

            // 所有会屏蔽交互的面板都关了 → 重新启用交互器
            if (_openedPanels.Count == 0)
            {
                SetInteractorsEnabled(true);
            }
        }

        // ========== 真正控制 collider 开关 ==========

        void SetInteractorsEnabled(bool enabled)
        {
            // 注意：Collider/Collider2D 是组件，没有 SetActive；我们用 enabled 打开/关闭
            foreach (var c in _colliders2D) if (c) c.enabled = enabled;
            foreach (var c in _colliders3D) if (c) c.enabled = enabled;
        }
    }
}
