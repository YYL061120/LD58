// Assets/Scripts/UI/Controllers/PanelOpenCloseNotifier.cs
using UnityEngine;

namespace DebtJam
{
    public class PanelOpenCloseNotifier : MonoBehaviour
    {
        [HideInInspector] public InteractableItemsController controller;

        void Awake()
        {
            if (!controller) controller = Object.FindFirstObjectByType<InteractableItemsController>();
        }

        void OnEnable() { controller?.NotifyPanelOpened(gameObject); }
        void OnDisable() { controller?.NotifyPanelClosed(gameObject); }
    }
}
