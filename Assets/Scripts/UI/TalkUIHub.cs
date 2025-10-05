using UnityEngine;
using UnityEngine.UI;

namespace DebtJam
{
    public class TalkUIHub : MonoBehaviour
    {
        [Header("Avatars")]
        public Image leftAvatar;   // 欠款人
        public Image rightAvatar;  // 玩家（可固定）

        [Header("Bubbles")]
        public TypewriterText leftBubble;
        public TypewriterText rightBubble;

        [Header("Options")]
        public Transform optionRoot;
        public Button optionButtonPrefab;

        string _debtorId;
        ActionCardSO _card;
        bool _isPhoneOrVisit; // true=逐字（电话/上门）；false=瞬显（短信）

        public void OpenCall(string id, ActionCardSO card) { _isPhoneOrVisit = true; Open(id, card); }
        public void OpenSMS(string id, ActionCardSO card) { _isPhoneOrVisit = false; Open(id, card); }
        public void OpenVisit(string id, ActionCardSO card) { _isPhoneOrVisit = true; Open(id, card); }

        void Open(string debtorId, ActionCardSO card)
        {
            _debtorId = debtorId; _card = card;
            var so = CaseManager.I.GetSO(debtorId);
            if (leftAvatar) leftAvatar.sprite = so.portrait;
            BuildOptions();
            gameObject.SetActive(true);
        }

        void ClearOptions() { foreach (Transform t in optionRoot) Destroy(t.gameObject); }

        void BuildOptions()
        {
            ClearOptions();
            foreach (var opt in _card.options)
            {
                var btn = Instantiate(optionButtonPrefab, optionRoot);
                btn.GetComponentInChildren<Text>().text = opt.optionText;
                btn.onClick.AddListener(() =>
                {
                    // 玩家台词（右）
                    if (_isPhoneOrVisit) rightBubble.Play(opt.optionText);
                    else rightBubble.Show(opt.optionText);

                    // 执行选项
                    if (!ActionExecutor.I.TryExecute(_card, opt, _debtorId, out var fail))
                        Debug.LogWarning("行动失败：" + fail);
                    // 欠款人回复由 ShowLineEffect 触发
                });
            }
        }

        // 给 ShowLineEffect 调用
        public void ShowLeft(string text) { if (_isPhoneOrVisit) leftBubble.Play(text); else leftBubble.Show(text); }
        public void ShowRight(string text) { if (_isPhoneOrVisit) rightBubble.Play(text); else rightBubble.Show(text); }
    }
}
