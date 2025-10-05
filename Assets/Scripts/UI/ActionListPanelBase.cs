using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace DebtJam
{
    public abstract class ActionListPanelBase : MonoBehaviour
    {
        [Header("UI")]
        public Transform contentRoot;
        public ContactEntryUI entryPrefab;
        public SimpleToast toast;

        [Header("Close")]
        public Button closeButton;          // ← 在 Inspector 里把“叉叉”按钮拖到这里
        public bool closeOnEscape = true;   // ← 按 Esc 关闭（可选）
        public bool closeOnBackground = false; // ← 若你有全屏半透明遮罩按钮，可勾上并把它也拖到 closeButton

        // 缓存委托，便于正确解绑
        System.Action rosterChangedHandler;
        System.Action<CaseRuntime> caseChangedHandler;

        protected virtual void Awake()
        {
            // 绑定关闭按钮
            if (closeButton)
                closeButton.onClick.AddListener(CloseSelf);

            // 备好事件委托（避免 OnDisable 解绑不上）
            rosterChangedHandler = Refresh;
            caseChangedHandler = _ => Refresh();
        }

        protected virtual void OnEnable()
        {
            if (CaseManager.I != null)
            {
                CaseManager.I.OnRosterChanged += rosterChangedHandler;
                CaseManager.I.OnCaseChanged += caseChangedHandler;
            }
            StartCoroutine(RefreshNextFrame());
        }

        protected virtual void OnDisable()
        {
            if (CaseManager.I != null)
            {
                CaseManager.I.OnRosterChanged -= rosterChangedHandler;
                CaseManager.I.OnCaseChanged -= caseChangedHandler;
            }

            // 可选：关闭时清空
            if (contentRoot)
            {
                for (int i = contentRoot.childCount - 1; i >= 0; i--)
                    Destroy(contentRoot.GetChild(i).gameObject);
            }
        }

        void Update()
        {
            if (!closeOnEscape) return;
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseSelf();
        }

        System.Collections.IEnumerator RefreshNextFrame()
        {
            yield return null;
            Refresh();
        }

        public void Refresh()
        {
            if (!contentRoot || !entryPrefab) return;

            if (CaseManager.I == null || CaseManager.I.runtimeById.Count == 0)
            {
                StartCoroutine(RefreshNextFrame());
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            IEnumerable<CaseRuntime> source = CaseManager.I.runtimeById.Values
                .Where(r => r.isVisible)
                .OrderBy(r => r.debtorId);

            int count = 0;
            foreach (var rt in source)
            {
                var so = CaseManager.I.GetSO(rt.debtorId);
                if (!so) continue;

                bool allowClick = GetAllowClick(rt);
                var item = Instantiate(entryPrefab, contentRoot);
                item.Setup(so, allowClick, OnEntryClicked);
                count++;
            }

            if (count == 0)
            {
                Debug.Log("[ActionListPanelBase] No visible debtors to show.");
            }
        }

        protected void ShowToast(string msg)
        {
            if (toast) toast.Show(msg);
            else Debug.Log(msg);
        }

        /// <summary>关闭当前面板（把这个 GameObject SetActive(false)）</summary>
        public void CloseSelf()
        {
            gameObject.SetActive(false);
        }

        // 子类需实现
        protected abstract bool GetAllowClick(CaseRuntime rt);
        protected abstract void OnEntryClicked(ContactEntryUI entry);
    }
}
