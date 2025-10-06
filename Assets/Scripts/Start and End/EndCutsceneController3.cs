using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace DebtJam
{
    public class EndCutsceneController_Scene3 : MonoBehaviour
    {
        [Header("UI References")]
        public Image bossImage;
        public TypewriterText typewriter;
        public TMP_Text dialogueText;

        [Header("Timing")]
        public float fadeDuration = 1f;
        public float autoWaitPerLine = 2.5f;

        [Header("Audio")]
        public AudioSource typeSound;
        public float soundCooldown = 6f;
        float lastSoundTime = -999f;

        bool isTyping = false;
        bool skipTyping = false;

        void Start()
        {
            // 确保 boss 初始隐藏
            if (bossImage)
            {
                Color c = bossImage.color;
                bossImage.color = new Color(c.r, c.g, c.b, 0f);
            }

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
            // Boss 淡入
            yield return StartCoroutine(FadeImage(bossImage, 0, 1, fadeDuration));

            yield return ShowLine("System report: Performance achieved.");
            yield return ShowLine("Total: Forty thousand.");
            yield return ShowLine("The boss’s voice sounds almost warm for once.");
            yield return ShowLine("‘Well done, Xiaobao. You finally made the numbers mean something.’");

            // Boss 淡出
            yield return StartCoroutine(FadeImage(bossImage, 1, 0, fadeDuration));

            // 玩家内心独白
            yield return ShowLine("You lean back in your chair.");
            yield return ShowLine("The fan hums like a distant wave.");
            yield return ShowLine("The rain outside has stopped.");
            yield return ShowLine("City lights shimmer across the window.");
            yield return ShowLine("You suddenly realize—");
            yield return ShowLine("Victory is just another kind of delayed payment.");

            // 等待玩家点击回主菜单
            while (!Input.GetMouseButtonDown(0))
                yield return null;

            SceneManager.LoadScene("Mainmenu");
        }

        IEnumerator ShowLine(string text)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            if (typeSound && Time.time - lastSoundTime > soundCooldown)
            {
                typeSound.Play();
                lastSoundTime = Time.time;
            }

            float dt = 1f / Mathf.Max(1f, typewriter.charsPerSecond);

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

            if (typeSound && typeSound.isPlaying)
                typeSound.Stop();

            isTyping = false;

            float elapsed = 0f;
            while (elapsed < autoWaitPerLine)
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
