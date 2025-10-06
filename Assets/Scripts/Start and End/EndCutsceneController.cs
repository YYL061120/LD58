using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace DebtJam
{
    public class EndCutsceneController : MonoBehaviour
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
            // Boss appears
            yield return StartCoroutine(FadeImage(bossImage, 0, 1, fadeDuration));

            yield return ShowLine("Boss:\n Mob... let me see how you did this week.");
            yield return ShowLine("(Pause)");
            yield return ShowLine("Oh no... only $10,000?");
            yield return ShowLine("Do you even realize what kind of situation our company is in right now?");
            yield return ShowLine("(He taps the table lightly)");
            yield return ShowLine("I don’t need excuses. I need results.");
            yield return ShowLine("You know there are plenty of people waiting to take your seat.");

            // Boss fades out
            yield return StartCoroutine(FadeImage(bossImage, 1, 0, fadeDuration));

            // Player inner thoughts
            yield return ShowLine("Me:\nThe air smells faintly of coffee and the hum of old machines.");
            yield return ShowLine("Suddenly, I remember my landlord’s unread message —");
            yield return ShowLine("‘Rent is due next week.’");
            yield return ShowLine("...");
            // 等待玩家点击退出
            while (!Input.GetMouseButtonDown(0))
                yield return null;
            yield return null;
            SceneManager.LoadScene("Mainmenu");
            //#if UNITY_EDITOR
            //UnityEditor.EditorApplication.isPlaying = false;
            //#else
            //Application.Quit();
            //#endif
        }

        IEnumerator ShowLine(string text)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            // 播放音效（每隔 soundCooldown 播放一次）
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

            // 自动等待时间 + 玩家可点击跳过
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
