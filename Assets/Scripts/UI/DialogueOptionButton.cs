// Assets/Scripts/UI/Dialogue/DialogueOptionButton.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DebtJam
{
    public class DialogueOptionButton : MonoBehaviour
    {
        public TMP_Text label;
        public Button btn;

        public void Setup(string text, System.Action onClick)
        {
            if (label) label.text = text;
            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}
