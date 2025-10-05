// Assets/Scripts/UI/Controllers/ScreenPanelButtons.cs
using UnityEngine;
using UnityEngine.UI;

namespace DebtJam
{
    /// <summary>
    /// Screen 面板内三个 icon 的统一桥接脚本。
    /// 在 Inspector 挂上按钮与控制器即可。
    /// </summary>
    public class ScreenPanelButtons : MonoBehaviour
    {
        public InteractableItemsController controller;

        [Header("Buttons")]
        public Button btnOpenSMS;
        public Button btnOpenProfile;
        public Button btnCloseScreen;

        void Awake()
        {
            if (!controller) controller = Object.FindFirstObjectByType<InteractableItemsController>();

            if (btnOpenSMS) btnOpenSMS.onClick.AddListener(() => controller?.OpenSMSFromScreen());
            if (btnOpenProfile) btnOpenProfile.onClick.AddListener(() => controller?.OpenProfileFromScreen());
            if (btnCloseScreen) btnCloseScreen.onClick.AddListener(() => controller?.CloseScreen());
        }
    }
}
