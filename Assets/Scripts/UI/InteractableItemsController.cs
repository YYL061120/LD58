// Assets/Scripts/UI/Controllers/InteractableItemsController.cs
using UnityEngine;

namespace DebtJam
{
    /// <summary>
    /// 集中管理“桌面上的可交互物体”打开的各类面板。
    /// 在 Inspector 里把对应的 Panel 拖进来。
    /// </summary>
    public class InteractableItemsController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject phonePanel;     // 电话面板
        public GameObject visitPanel;     // 地址/上门面板
        public GameObject screenPanel;    // 屏幕（桌面应用全屏）
        public GameObject smsPanel;       // 短信面板（从Screen里点开）
        public GameObject profilePanel;   // Profile面板（从Screen里点开）

        [Header("Optional")]
        public GameObject[] othersToHideWhenScreenOpen; // 打开Screen时想一起隐藏的UI

        void SetActiveSafe(GameObject go, bool active)
        {
            if (go && go.activeSelf != active) go.SetActive(active);
        }

        public void OpenPhone()
        {
            SetActiveSafe(phonePanel, true);
        }

        public void OpenVisit()
        {
            SetActiveSafe(visitPanel, true);
        }

        public void OpenScreen()
        {
            SetActiveSafe(screenPanel, true);
            // 可选：打开Screen时把桌面上的一些提示UI关掉
            if (othersToHideWhenScreenOpen != null)
                foreach (var go in othersToHideWhenScreenOpen) SetActiveSafe(go, false);
        }

        public void CloseScreen()
        {
            SetActiveSafe(screenPanel, false);
            // 关闭Screen后如果需要把某些UI恢复，也可以在这里 SetActive(true)
        }

        public void OpenSMSFromScreen()
        {
            SetActiveSafe(smsPanel, true);
        }

        public void OpenProfileFromScreen()
        {
            SetActiveSafe(profilePanel, true);
        }
    }
}
