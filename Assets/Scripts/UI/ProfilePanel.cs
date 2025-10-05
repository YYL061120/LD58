using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DebtJam
{
    /// <summary>
    /// 把“当前可显示的欠款人（isVisible==true 且 outcome==Pending）”列出来，
    /// 用 VerticalLayoutGroup 排列。点击列表项会 SelectCase，并在右侧（如对话/通讯录）联动。
    /// </summary>
    public class ProfilePanel : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("放置条目的父节点，挂 VerticalLayoutGroup + ContentSizeFitter")]
        public Transform contentRoot;

        [Tooltip("每个档案条目的预制体（必须带 ProfileItemUI 组件 + Button）")]
        public ProfileItemUI itemPrefab;

        [Header("选项")]
        [Tooltip("是否隐藏已 DeadEnd 的案件（一般也算完结，建议勾选）")]
        public bool hideDeadEnd = true;

        [Tooltip("当没有任何可展示的档案时，显示的占位文本（可选）")]
        public GameObject emptyStateGO;

        // 维护当前列表，避免反复 GC
        private readonly List<ProfileItemUI> _items = new();

        void OnEnable()
        {
            // 订阅 CaseManager 事件：名册变化（解锁）、当前案件变化（可用来高亮）
            CaseManager.I.OnRosterChanged += Refresh;
            CaseManager.I.OnCaseChanged += OnCaseChanged;
            Refresh();
        }

        void OnDisable()
        {
            if (CaseManager.I == null) return;
            CaseManager.I.OnRosterChanged -= Refresh;
            CaseManager.I.OnCaseChanged -= OnCaseChanged;
        }

        void OnCaseChanged(CaseRuntime _)
        {
            // 仅更新高亮，不重建列表
            HighlightCurrent();
        }

        /// <summary>重建列表</summary>
        public void Refresh()
        {
            if (contentRoot == null || itemPrefab == null) return;

            // 先清空旧的
            foreach (var it in _items) if (it) Destroy(it.gameObject);
            _items.Clear();

            var cm = CaseManager.I;
            // 过滤规则：显示 isVisible==true 且 outcome==Pending（未完结）
            IEnumerable<CaseRuntime> candidates = cm.runtimeById.Values
                .Where(r => r.isVisible && r.outcome == CaseOutcome.Pending);

            // 如需把 DeadEnd 也隐藏（默认 true），否则可以把 DeadEnd 也列出来
            if (!hideDeadEnd)
            {
                candidates = cm.runtimeById.Values.Where(r => r.isVisible && r.outcome != CaseOutcome.Collected);
            }

            var list = candidates.OrderBy(r => r.debtorId).ToList();

            // 空态
            if (emptyStateGO) emptyStateGO.SetActive(list.Count == 0);

            foreach (var rt in list)
            {
                var so = cm.GetSO(rt.debtorId);
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(rt, so, OnClickItem);
                _items.Add(item);
            }

            // 构建后更新一次高亮
            HighlightCurrent();
        }

        private void OnClickItem(CaseRuntime rt)
        {
            CaseManager.I.SelectCase(rt.debtorId);
            // 这里不切换面板，仅选择当前对象；其它 UI（电话/短信/上门）会读取 currentCase
            HighlightCurrent();
        }

        private void HighlightCurrent()
        {
            var current = CaseManager.I.currentCase;
            foreach (var it in _items)
            {
                bool isCurrent = (current != null && it != null && it.Runtime == current);
                it.SetHighlight(isCurrent);
            }
        }
    }
}


