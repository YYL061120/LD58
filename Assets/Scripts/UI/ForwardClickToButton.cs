// Assets/Scripts/UI/DialogueRelated/ForwardClickToButton.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebtJam
{
    /// <summary>
    /// 把父物体的点击转发到子 Button（父物体通常是更大的“可点范围”）
    /// </summary>
    [DisallowMultipleComponent]
    public class ForwardClickToButton : MonoBehaviour, IPointerClickHandler
    {
        public Button target;

        // 注意：要能收到点击，当前物体需要有 Graphic（Image/RawImage/Text...）并且 Raycast Target = true
        // TalkUIHub 会在运行时自动加一个透明 Image 来保证能接收点击。

        public void OnPointerClick(PointerEventData eventData)
        {
            if (target != null && target.interactable && target.gameObject.activeInHierarchy)
                target.onClick.Invoke();
        }
    }
}
