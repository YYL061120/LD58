using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DebtJam
{
    public class StartMenuController : MonoBehaviour
    {
        [Header("UI References")]
        public Button startButton;
        public AudioSource clickSound;

        void Start()
        {
            if (startButton)
            {
                startButton.interactable = false; // 启动时暂时禁用按钮
                Invoke(nameof(EnableButton), 0.1f); // 半秒后启用按钮，避免误触
            }
        }

        void EnableButton()
        {
            if (startButton)
            {
                startButton.interactable = true;
                startButton.onClick.AddListener(OnStartClicked);
            }
        }

        void OnStartClicked()
        {
            if (clickSound)
                clickSound.Play();

            SceneManager.LoadScene("Start_Scene");
        }
    }
}
