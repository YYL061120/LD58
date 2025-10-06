// Assets/Scripts/UI/TypewriterTMP.cs
using UnityEngine;
using TMPro;
using System.Collections;

namespace DebtJam
{
    public class TypewriterTMP : MonoBehaviour
    {
        public float charsPerSecond = 36f;

        public IEnumerator PlayCoroutine(TMP_Text target, string text)
        {
            if (!target) yield break;
            target.text = "";
            float dt = 1f / Mathf.Max(1f, charsPerSecond);
            foreach (char c in text)
            {
                target.text += c;
                yield return new WaitForSecondsRealtime(dt);
            }
        }

        // 便捷方法：直接开始一个协程（当脚本挂在某对象上时）
        public void Play(TMP_Text target, string text)
        {
            StartCoroutine(PlayCoroutine(target, text));
        }
    }
}
