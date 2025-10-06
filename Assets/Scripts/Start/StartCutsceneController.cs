using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace DebtJam
{
    public class StartCutsceneController : MonoBehaviour
    {
        [Header("UI References")]
        public Image bossImage;
        public TypewriterText typewriter;
        public TMP_Text dialogueText;

        [Header("Timing")]
        public float fadeDuration = 1f;
        public float waitAfterLine = 1f;

        bool isTyping = false;
        bool skipTyping = false;

        void Start()
        {
            StartCoroutine(PlayCutscene());
        }

        void Update()
        {
            // 点击跳过正在播放的文字
            if (Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                    skipTyping = true;
            }
        }

        IEnumerator PlayCutscene()
        {
            // 老板出现
            yield return StartCoroutine(FadeImage(bossImage, 0, 1, fadeDuration));

            // 老板台词
            yield return ShowLine("Boss:\n Xiaobao, you know the company is in a pretty bad financial spot lately.");
            yield return ShowLine("If you don’t collect enough debts this week, I might have to let you go.");
            yield return ShowLine("But if you do well… I’ll make sure you’re rewarded.");
            yield return ShowLine("Now, get to work!");

            // 老板淡出
            yield return StartCoroutine(FadeImage(bossImage, 1, 0, fadeDuration));

            // 玩家独白
            yield return ShowLine("Me:\n“Man, the boss is laying it on thick again…");
            yield return ShowLine("Whatever, better just get to work.");
            yield return ShowLine("Rent’s due in less than a week—");
            yield return ShowLine("and I’m not planning to sleep on the street.。");

            // 延迟一点再切场景
            while (!Input.GetMouseButtonDown(0))
                yield return null;

            SceneManager.LoadScene("Game_Place");
        }

        IEnumerator ShowLine(string text)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            float dt = 1f / Mathf.Max(1f, typewriter.charsPerSecond);

            // 打字机播放
            foreach (char c in text)
            {
                if (skipTyping)
                {
                    dialogueText.text = text;
                    break;
                }

                dialogueText.text += c;
                yield return new WaitForSeconds(dt);
            }

            isTyping = false;

            // 自动等待时间 + 可点击跳过逻辑
            float elapsed = 0f;
            float autoWait = 5f; // 等待 2.5 秒后自动进入下一句
            while (elapsed < autoWait)
            {
                if (Input.GetMouseButtonDown(0)) // 玩家点击立即跳过等待
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }


        IEnumerator FadeImage(Image img, float from, float to, float duration)
        {
            float t = 0f;
            Color c = img.color;
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, t / duration);
                img.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
        }
    }
}
