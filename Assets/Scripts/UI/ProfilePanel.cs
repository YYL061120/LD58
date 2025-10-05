// Assets/Scripts/UI/ProfilePanel_DisplayOnly_TMP.cs
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebtJam
{
    /// <summary>
    /// 只读的 Profile 列表（TextMeshPro），支持“关闭按钮/ESC 关闭”
    /// </summary>
    public class ProfilePanel_DisplayOnly_TMP : MonoBehaviour
    {
        [Header("UI")]
        public Transform contentRoot;                   // 列表容器（Vertical Layout Group）
        public ProfileItemDisplay_TMP itemPrefab;       // 每一项的预制（里头有头像/标题/词条）
        public GameObject emptyStateGO;                 // 列表为空时显示（可选）

        [Header("Close")]
        public Button closeButton;                      // 叉叉按钮（可选）
        public bool closeOnEscape = true;               // 按 Esc 关闭

        // 缓存已生成的项，方便清理
        readonly List<ProfileItemDisplay_TMP> _spawned = new();

        // 事件委托缓存，避免解绑不上
        System.Action rosterChangedHandler;

        void Awake()
        {
            rosterChangedHandler = Refresh;

            if (closeButton)
                closeButton.onClick.AddListener(CloseSelf);
        }

        void OnEnable()
        {
            if (CaseManager.I != null)
                CaseManager.I.OnRosterChanged += rosterChangedHandler;

            // 稳妥：晚一帧刷新，避免 CaseManager 还没 Build
            StartCoroutine(RefreshNextFrame());
        }

        void OnDisable()
        {
            if (CaseManager.I != null)
                CaseManager.I.OnRosterChanged -= rosterChangedHandler;

            // 可选：关闭时清空
            ClearAll();
        }

        void Update()
        {
            if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
                CloseSelf();
        }

        System.Collections.IEnumerator RefreshNextFrame()
        {
            yield return null;
            Refresh();
        }

        public void CloseSelf()
        {
            gameObject.SetActive(false);
        }

        void ClearAll()
        {
            for (int i = _spawned.Count - 1; i >= 0; i--)
                if (_spawned[i]) Destroy(_spawned[i].gameObject);
            _spawned.Clear();
        }

        /// <summary>重建可见案件的 Profile 列表</summary>
        public void Refresh()
        {
            if (!contentRoot || !itemPrefab) return;

            // CaseManager 还没建立 → 晚一帧再试
            if (CaseManager.I == null || CaseManager.I.runtimeById.Count == 0)
            {
                StartCoroutine(RefreshNextFrame());
                return;
            }

            ClearAll();

            IEnumerable<CaseRuntime> source = CaseManager.I.runtimeById.Values
                .Where(r => r.isVisible)                    // 只显示“出现在 Profile 的人”
                .OrderBy(r => r.debtorId);

            int count = 0;
            foreach (var rt in source)
            {
                var so = CaseManager.I.GetSO(rt.debtorId);
                if (!so) continue;

                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(rt, so);                         // 你的项脚本已有此方法
                _spawned.Add(item);
                count++;
            }

            if (emptyStateGO) emptyStateGO.SetActive(count == 0);
            if (count == 0) Debug.Log("[ProfilePanel] list is empty.");
        }
    }
}
