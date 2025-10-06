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
//facts
//    P
//Vector3(-43.5, 27.5799999, 0)
//Vector3(-89.0999985, 27.6000004, 0)
//S
//    Vector3(1.08000004, 1.22227263, 1.22227263)

//Title
//    S
//    Vector3(1.00999999,1.34444451,1.34444451)
//    P
//    Vector3(-19.5,72,0)
//Vector3(466.899994, -334.029999, 0)
//    thickness 0.2