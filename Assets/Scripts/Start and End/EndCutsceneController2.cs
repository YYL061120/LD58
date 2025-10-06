using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace DebtJam
{
    public class EndCutscene_Best : MonoBehaviour
    {
        [Header("UI References")]
        public Image bossImage;
        public TypewriterText typewriter;
        public TMP_Text dialogueText;

        [Header("Timing")]
        public float fadeDuration = 1f;
        public float autoWaitPerLine = 2.5f; // 节奏更慢一点

        [Header("Audio")]
        public AudioSource typeSound;
        public AudioSource bgm;             // 背景音乐
        public float soundCooldown = 6f;
        float lastSoundTime = -999f;

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
            // BGM 渐入
            if (bgm)
            {
                bgm.volume = 0;
                bgm.Play();
                yield return StartCoroutine(FadeAudio(bgm, 0, 0.5f, 2f));
            }

            // Boss 出现
            yield return StartCoroutine(FadeImage(bossImage, 0, 1, fadeDuration));

            yield return ShowLine("“Report uploaded.”");
            yield return ShowLine("“Total: Twenty-five thousand.”");
            yield return ShowLine("The boss appears again — calmer this time.");
            yield return ShowLine("Boss:\nHmm… not bad. At least the company will survive a few more days.");

            // Boss 淡出
            yield return StartCoroutine(FadeImage(bossImage, 1, 0, fadeDuration));

            // 玩家内心独白
            yield return ShowLine("You smile.");
            yield return ShowLine("Not sure if it’s relief or resignation.");
            yield return ShowLine("The coffee on your desk has gone cold.");
            yield return ShowLine("Its surface reflects your tired face.");
            yield return ShowLine("You close the laptop.");
            yield return ShowLine("The office lights are still on.");
            yield return ShowLine("Outside, it’s already night.");

            // 等待点击返回主菜单
            while (!Input.GetMouseButtonDown(0))
                yield return null;

            // BGM 渐出
            if (bgm)
                yield return StartCoroutine(FadeAudio(bgm, bgm.volume, 0, 2f));

            SceneManager.LoadScene("Mainmenu");
        }

        IEnumerator ShowLine(string text)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            // 播放一次打字音效
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

            // 等待玩家阅读或点击跳过
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

        IEnumerator FadeAudio(AudioSource audio, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                audio.volume = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
        }
    }
}
