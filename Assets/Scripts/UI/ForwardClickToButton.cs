using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DebtJam
{
    /// <summary>
    /// 挂在“父物体(hitbox)”上；把点击转发给 target Button。
    /// 避免双击：同一帧/进行中只发一次。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ForwardClickToButton : MonoBehaviour, IPointerClickHandler
    {
        public Button target;

        // 防抖：一次点击串行期间只触发一次
        bool _busy;

        void Reset()
        {
            var img = GetComponent<Image>();
            if (img)
            {
                var c = img.color; c.a = 0f; img.color = c; // 透明但可点
                img.raycastTarget = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!target || !target.interactable) return;
            if (_busy) return;

            _busy = true;
            try
            {
                target.onClick?.Invoke();
            }
            finally
            {
                // 下一帧再允许下一次（避免同帧多次）
                StartCoroutine(ClearBusyNextFrame());
            }
        }

        System.Collections.IEnumerator ClearBusyNextFrame()
        {
            yield return null;
            _busy = false;
        }
    }
}
