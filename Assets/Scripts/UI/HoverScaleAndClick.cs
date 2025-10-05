// Assets/Scripts/UI/Controllers/HoverScaleAndClick.cs
using UnityEngine;
using UnityEngine.EventSystems;

namespace DebtJam
{
    /// <summary>
    /// 悬停平滑放大 + 点击触发控制器的某个动作。
    /// 既支持场景精灵(需 Collider2D)的 OnMouseXXX，也支持 UI 的 IPointerXXX。
    /// </summary>
    public class HoverScaleAndClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public enum ClickAction { None, OpenPhone, OpenVisit, OpenScreen }

        [Header("Action")]
        public InteractableItemsController controller;
        public ClickAction action;

        [Header("Hover Scale")]
        public float hoverScaleMultiplier = 1.08f;
        public float scaleLerpSpeed = 10f;

        Vector3 _baseScale;
        Vector3 _targetScale;
        bool _hover;

        void Awake()
        {
            _baseScale = transform.localScale;
            _targetScale = _baseScale;
            // 如果没拖控制器，尝试在场景中找
            if (!controller) controller = Object.FindFirstObjectByType<InteractableItemsController>();
        }

        void Update()
        {
            var desired = _hover ? _baseScale * hoverScaleMultiplier : _baseScale;
            _targetScale = Vector3.Lerp(_targetScale, desired, Time.deltaTime * scaleLerpSpeed);
            transform.localScale = _targetScale;
        }

        // -------- UI 事件路径（Canvas 下 Image/Button） --------
        public void OnPointerEnter(PointerEventData eventData) => _hover = true;
        public void OnPointerExit(PointerEventData eventData) => _hover = false;
        public void OnPointerClick(PointerEventData eventData) => DoClick();

        // -------- 场景精灵路径（SpriteRenderer + Collider2D）--------
        void OnMouseEnter() { _hover = true; }
        void OnMouseExit() { _hover = false; }
        void OnMouseDown() { DoClick(); }

        void DoClick()
        {
            if (!controller) return;

            switch (action)
            {
                case ClickAction.OpenPhone: controller.OpenPhone(); break;
                case ClickAction.OpenVisit: controller.OpenVisit(); break;
                case ClickAction.OpenScreen: controller.OpenScreen(); break;
                default: break;
            }
        }
    }
}
