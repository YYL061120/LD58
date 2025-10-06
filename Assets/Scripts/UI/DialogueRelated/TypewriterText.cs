// Assets/Scripts/UI/TypewriterText.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebtJam
{
    public class TypewriterText : MonoBehaviour
    {
        [Header("Target (可不填，Awake 自动搜)")]
        public TMP_Text tmp;
        public Text legacy;

        [Range(1f, 120f)] public float charsPerSecond = 30f;

        Coroutine co;
        string _full = "";
        public bool IsTyping { get; private set; }

        void Awake()
        {
            if (!tmp && !legacy)
            {
                tmp = GetComponent<TMP_Text>();
                if (!tmp) legacy = GetComponent<Text>();
            }
        }

        bool TargetActive()
        {
            var go = tmp ? tmp.gameObject : legacy ? legacy.gameObject : null;
            return go && go.activeInHierarchy && isActiveAndEnabled;
        }

        string GetText()
        {
            if (tmp) return tmp.text;
            if (legacy) return legacy.text;
            return "";
        }

        void Set(string s)
        {
            if (tmp) tmp.text = s;
            else if (legacy) legacy.text = s;
        }

        public void Clear()
        {
            if (co != null) StopCoroutine(co);
            co = null;
            IsTyping = false;
            _full = "";
            Set("");
        }

        public void Show(string s)
        {
            if (co != null) StopCoroutine(co);
            co = null;
            IsTyping = false;
            _full = s ?? "";
            Set(_full);
        }

        public void Play(string s)
        {
            if (!TargetActive()) { Show(s); return; }  // 未激活直接瞬显，防止协程报错

            if (co != null) StopCoroutine(co);
            co = StartCoroutine(CoType(s ?? ""));
        }

        public void SkipToEnd()
        {
            if (co != null) StopCoroutine(co);
            co = null;
            IsTyping = false;
            Set(_full);
        }

        IEnumerator CoType(string s)
        {
            _full = s;
            IsTyping = true;
            Set("");
            float dt = 1f / Mathf.Max(1f, charsPerSecond);

            // 简易富文本支持：遇到 <tag> 一次性写入
            for (int i = 0; i < s.Length;)
            {
                if (s[i] == '<')
                {
                    int end = s.IndexOf('>', i);
                    if (end >= 0)
                    {
                        Set(GetText() + s.Substring(i, end - i + 1));
                        i = end + 1;
                        yield return null;
                        continue;
                    }
                }

                Set(GetText() + s[i]);
                i++;
                yield return new WaitForSecondsRealtime(dt);
            }

            IsTyping = false;
            co = null;
        }
    }
}
