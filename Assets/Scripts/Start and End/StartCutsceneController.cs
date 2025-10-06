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

        [Header("Audio")]
        public AudioSource typeSound; // 键盘音效（持续音）
        public float soundCooldown = 6f; // 每隔几秒才可再次播放一次
        private float lastSoundTime = -999f;

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
            if (Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                    skipTyping = true;
            }
        }

        IEnumerator PlayCutscene()
        {
            yield return StartCoroutine(FadeImage(bossImage, 0, 1, fadeDuration));

            yield return ShowLine("Boss:\n Mob, you know the company is in a pretty bad financial spot lately.");
            yield return ShowLine("If you don’t collect enough debts this week, I might have to let you go.");
            yield return ShowLine("But if you do well… I’ll make sure you’re rewarded.");
            yield return ShowLine("Now, get to work!");

            yield return StartCoroutine(FadeImage(bossImage, 1, 0, fadeDuration));

            yield return ShowLine("Me:\n“Man, the boss is laying it on thick again…");
            yield return ShowLine("Whatever, better just get to work.");
            yield return ShowLine("Rent is due in less than a week—");
            yield return ShowLine("and I’m not planning to sleep on the street.");

            while (!Input.GetMouseButtonDown(0))
                yield return null;

            SceneManager.LoadScene("Game Start");
        }

        IEnumerator ShowLine(string text)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            float dt = 1f / Mathf.Max(1f, typewriter.charsPerSecond);

            // 🔊 在打字开始时播放一次音效（如果冷却完毕）
            if (typeSound && Time.time - lastSoundTime > soundCooldown)
            {
                typeSound.Play();
                lastSoundTime = Time.time;
            }

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

            // ✅ 打字结束后停止音效播放
            if (typeSound && typeSound.isPlaying)
                typeSound.Stop();

            isTyping = false;

            float elapsed = 0f;
            float autoWait = 3f;
            while (elapsed < autoWait)
            {
                if (Input.GetMouseButtonDown(0))
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
