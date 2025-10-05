// Assets/Scripts/UI/VisitTransition.cs
using UnityEngine;
using System.Collections;

namespace DebtJam
{
    public class VisitTransition : MonoBehaviour
    {
        public CanvasGroup fade; // 可选：黑幕
        public float fadeTime = 0.4f;

        public IEnumerator PlayThenBegin(CaseRuntime rt, ActionCardSO card, TalkUIHub hub)
        {
            // 1) 关闭 VisitPanel（外部调用时先做）
            // 2) 播一个简单淡入
            if (fade)
            {
                fade.gameObject.SetActive(true);
                for (float t = 0; t < fadeTime; t += Time.deltaTime)
                {
                    fade.alpha = Mathf.InverseLerp(0, fadeTime, t);
                    yield return null;
                }
                fade.alpha = 1;
            }

            // TODO：在这里播放车子移动的动画/音效

            // 3) 开始对话（用泡泡样式）
            hub.OpenVisit(rt.debtorId, card);

            // 4) 淡出
            if (fade)
            {
                for (float t = 0; t < fadeTime; t += Time.deltaTime)
                {
                    fade.alpha = 1 - Mathf.InverseLerp(0, fadeTime, t);
                    yield return null;
                }
                fade.alpha = 0;
                fade.gameObject.SetActive(false);
            }
        }
    }
}
