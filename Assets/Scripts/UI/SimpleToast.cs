// Assets/Scripts/UI/Shared/SimpleToast.cs
using UnityEngine;
using TMPro;
using System.Collections;

namespace DebtJam
{
    public class SimpleToast : MonoBehaviour
    {
        public TMP_Text text;
        public CanvasGroup cg;
        public float hold = 1.2f;
        public float fade = 0.4f;

        void Awake()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        public void Show(string msg)
        {
            if (text) text.text = msg;
            StopAllCoroutines();
            StartCoroutine(DoShow());
        }

        IEnumerator DoShow()
        {
            cg.alpha = 1f;
            yield return new WaitForSeconds(hold);
            float t = 0f;
            while (t < fade)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, t / fade);
                yield return null;
            }
            cg.alpha = 0f;
        }
    }
}
