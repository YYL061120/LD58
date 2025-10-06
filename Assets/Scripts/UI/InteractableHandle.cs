// Assets/Scripts/UI/Interactables/InteractableHandle.cs
using UnityEngine;

namespace DebtJam
{
    [DisallowMultipleComponent]
    public class InteractableHandle : MonoBehaviour
    {
        [Header("Refs")]
        public Collider2D col;                 // 如 BoxCollider2D
        public MonoBehaviour hoverScaleScript; // 你的 HoverScaleAndClick 脚本（可选）

        [Header("Optional Visual")]
        public SpriteRenderer dimSprite;       // 若希望禁用时微灰
        [Range(0f, 1f)] public float dimAlpha = 0.6f;

        bool _lastInteractable = true;
        Color _origColor;

        void Reset()
        {
            col = GetComponent<Collider2D>();
            dimSprite = GetComponent<SpriteRenderer>();
        }

        void Awake()
        {
            if (!col) col = GetComponent<Collider2D>();
            if (dimSprite) _origColor = dimSprite.color;
        }

        public void SetInteractable(bool on)
        {
            _lastInteractable = on;

            if (col) col.enabled = on;
            if (hoverScaleScript) hoverScaleScript.enabled = on;

            if (dimSprite)
            {
                var c = _origColor;
                c.a = on ? _origColor.a : _origColor.a * dimAlpha;
                dimSprite.color = c;
            }
        }

        public bool IsInteractable() => _lastInteractable;
    }
}
