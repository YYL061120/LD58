//// Assets/Scripts/UI/TypewriterText.cs
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;

//namespace DebtJam
//{
//    public class TypewriterText : MonoBehaviour
//    {
//        [Header("Targets (任选其一)")]
//        public Text uiText;
//        public TMP_Text tmpText;

//        [Header("Speed")]
//        public float charsPerSecond = 28f;

//        Coroutine co;

//        void SetText(string s)
//        {
//            if (tmpText) tmpText.text = s;
//            if (uiText) uiText.text = s;
//        }
//        string GetText()
//        {
//            if (tmpText) return tmpText.text;
//            if (uiText) return uiText.text;
//            return "";
//        }

//        public void Play(string text)
//        {
//            if (co != null) StopCoroutine(co);
//            co = StartCoroutine(CoType(text));
//        }

//        public void Show(string text)
//        {
//            if (co != null) StopCoroutine(co);
//            SetText(text);
//        }

//        public void Clear()
//        {
//            if (co != null) StopCoroutine(co);
//            SetText("");
//        }

//        IEnumerator CoType(string s)
//        {
//            SetText("");
//            float dt = 1f / Mathf.Max(1f, charsPerSecond);

//            // 简易富文本：标签一次性写入
//            int i = 0;
//            while (i < s.Length)
//            {
//                if (s[i] == '<')
//                {
//                    int end = s.IndexOf('>', i);
//                    if (end >= 0)
//                    {
//                        SetText(GetText() + s.Substring(i, end - i + 1));
//                        i = end + 1;
//                        continue;
//                    }
//                }

//                SetText(GetText() + s[i]);
//                i++;
//                yield return new WaitForSeconds(dt);
//            }
//        }
//    }
//}
// Assets/Scripts/UI/TypewriterText.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace DebtJam
{
    public class TypewriterText : MonoBehaviour
    {
        [Header("Target (可不填，Awake 自动搜)")]
        public TMP_Text tmp;
        public Text legacy;

        [Range(1f, 120f)] public float charsPerSecond = 30f;

        Coroutine co;

        void Awake()
        {
            if (!tmp && !legacy)
            {
                tmp = GetComponent<TMP_Text>();
                if (!tmp) legacy = GetComponent<Text>();
            }
        }

        void SetTextInternal(string s)
        {
            if (tmp) tmp.text = s;
            else if (legacy) legacy.text = s;
        }

        public void Clear()
        {
            if (co != null) StopCoroutine(co);
            SetTextInternal("");
        }

        public void Show(string s)
        {
            if (co != null) StopCoroutine(co);
            SetTextInternal(s);
        }

        public void Play(string s)
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(CoType(s));
        }

        IEnumerator CoType(string s)
        {
            SetTextInternal("");
            float dt = 1f / Mathf.Max(charsPerSecond, 1f);
            foreach (char c in s)
            {
                if (tmp) tmp.text += c;
                else if (legacy) legacy.text += c;
                yield return new WaitForSeconds(dt);
            }
            co = null;
        }
    }
}

