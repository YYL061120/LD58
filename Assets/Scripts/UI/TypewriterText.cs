using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DebtJam
{
    public class TypewriterText : MonoBehaviour
    {
        public Text target;
        public float charsPerSecond = 30f;
        Coroutine co;

        public void Play(string text)
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(CoType(text));
        }

        IEnumerator CoType(string s)
        {
            target.text = "";
            float dt = 1f / Mathf.Max(1f, charsPerSecond);
            foreach (char c in s)
            {
                target.text += c;
                yield return new WaitForSeconds(dt);
            }
        }

        public void Show(string text)
        {
            if (co != null) StopCoroutine(co);
            target.text = text;
        }
    }
}
