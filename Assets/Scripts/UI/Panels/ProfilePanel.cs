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
            if (CaseManager.I != null) CaseManager.I.OnRosterChanged += Rebuild;
            Rebuild();
        }
        void OnDisable()
        {
            if (CaseManager.I != null) CaseManager.I.OnRosterChanged -= Rebuild;
        }

        void Rebuild()
        {
            // 1) 清空
            foreach (Transform t in contentRoot) Destroy(t.gameObject);

            // 2) 仅渲染：可见 && Pending 的案件
            foreach (var rt in CaseManager.I.GetVisiblePendingCases())
            {
                var so = CaseManager.I.GetSO(rt.debtorId);
                var item = Instantiate(itemPrefab, contentRoot);

                // 基本显示
                item.Setup(rt, so); // 你的现有 API

                // 面板各自的启用条件（例）：
                // PhonePanel：btn.interactable = rt.hasPhone;
                // SMSPanel：  btn.interactable = rt.hasPhone;
                // VisitPanel：btn.interactable = rt.hasAddress; 若无地址 → 置灰并显示提示
            }
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
